using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Paracosm.Common.Utils;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Terraria.DataStructures;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Projectiles;
using Paracosm.Common.Systems;
using Terraria.GameContent.Bestiary;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Ranged;
using Paracosm.Content.Items.Weapons.Summon;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Paracosm.Content.Projectiles.Hostile;
using System.Linq;
using Paracosm.Content.Buffs;
using System.IO;
using Terraria.Graphics.Shaders;


namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class SolarChampion : ModNPC
    {
        public ref float AITimer => ref NPC.ai[0];
        public ref float Attack => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float AttackCount => ref NPC.ai[3];

        public int AttackTimer2 = 0;
        public int AttackCount2 = 0;

        bool phase2FirstTime = false;
        int phase = 1;

        public float attackDuration = 0;
        int[] attackDurations = { 1200, 900, 1200, 600 };
        int[] attackDurations2 = { 1200, 900, 1200, 600 };
        Player player;
        Vector2 playerDirection;
        float randomNum = 0;
        Vector2 tempPos = Vector2.Zero;
        Vector2 leftOffset = new Vector2(-500, 0);
        Vector2 targetPosition = Vector2.Zero;
        float swordOffset = 200;
        float rotSpeedMul = 1;
        bool isDashing = false;
        bool grow = true;
        float arenaDistance = 0;
        List<Projectile> Spheres = new List<Projectile>();
        List<Projectile> Swords = new List<Projectile>();
        List<Projectile> Axes = new List<Projectile>();

        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sword", ModContent.ProjectileType<SolarBlade>()},
            {"Hammer", ModContent.ProjectileType<SolarHammer>()},
            {"Axe", ModContent.ProjectileType<SolarAxe>()},
            {"Fireball", ModContent.ProjectileType<SolarFireball>()},
            {"Sphere", ModContent.ProjectileType<SolarSphere>()}
        };


        enum Attacks
        {
            DashingSword,
            OrbitingSwords,
            AxeSpin,
            HammerStorm
        }

        enum Attacks2
        {
            DashingSword2,
            OrbitingSwords2,
            AxeSpin2,
            HammerStorm2
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            /* NPCID.Sets.NPCBestiaryDrawModifiers drawMod = new NPCID.Sets.NPCBestiaryDrawModifiers()
             {
                 PortraitPositionYOverride = -15f,
                 PortraitScale = 0.8f
             };
             NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawMod);*/
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("A monster who arrived from a different world. Though feral in nature, it possesses high intelligence, suggesting its arrival was not without purpose."),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 100;
            NPC.height = 100;
            NPC.Opacity = 0;
            NPC.lifeMax = 400000;
            NPC.defense = 100;
            NPC.damage = 20;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 100000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SeveredSpace");
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPosition.X);
            writer.Write(targetPosition.Y);
            writer.Write(randomNum);
            writer.Write(attackDuration);
            writer.Write(phase);
            writer.Write(phase2FirstTime);
            writer.Write(AttackTimer2);
            writer.Write(AttackCount2);
            writer.Write(rotSpeedMul);
            writer.Write(grow);

            writer.Write(Swords.Count);
            writer.Write(Axes.Count);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPosition.X = reader.ReadSingle();
            targetPosition.Y = reader.ReadSingle();
            randomNum = reader.ReadSingle();
            attackDuration = reader.ReadSingle();
            phase = reader.ReadInt32();
            phase2FirstTime = reader.ReadBoolean();
            AttackTimer2 = reader.ReadInt32();
            AttackCount2 = reader.ReadInt32();
            rotSpeedMul = reader.ReadSingle();
            grow = reader.ReadBoolean();

            int count1 = reader.ReadInt32();
            int swordCounter = 0;
            Swords.Clear();
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Proj["Sword"])
                {
                    Swords.Add(proj);
                    swordCounter++;
                    if (swordCounter >= count1)
                    {
                        break;
                    }
                }
            }

            int count2 = reader.ReadInt32();
            int axeCounter = 0;
            Axes.Clear();
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Proj["Axe"])
                {
                    Axes.Add(proj);
                    axeCounter++;
                    if (axeCounter >= count2)
                    {
                        break;
                    }
                }
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];

            foreach (var p in Main.player)
            {
                p.solarMonolithShader = true;
            }
            playerDirection = (player.Center - NPC.Center);

            if (player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000)
            {
                NPC.active = false;
            }
            Arena();
            if (!Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(255, 192, 100));
            }

            if (AITimer < 60)
            {
                NPC.dontTakeDamage = true;
                NPC.velocity = new Vector2(0, -2);
                NPC.Opacity += (1f / 60f);
                AITimer++;
                Attack = -1;
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.life <= (NPC.lifeMax / 2) && !phase2FirstTime)
            {
                phase2FirstTime = true;
                phase = 2;
                SwitchAttacks();
                NPC.netUpdate = true;
            }


            if (NPC.velocity.Length() > 10)
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, playerDirection.X * MathHelper.ToRadians(30), MathHelper.ToRadians(1));
            }
            else
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, MathHelper.ToRadians(3));
            }

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.DashingSword:
                        DashingSword(player);
                        break;
                    case (int)Attacks.OrbitingSwords:
                        OrbitingSwords(player);
                        break;
                    case (int)Attacks.AxeSpin:
                        AxeSpin(player);
                        break;
                    case (int)Attacks.HammerStorm:
                        HammerStorm(player);
                        break;
                }
            }
            else
            {
                switch (Attack)
                {
                    case (int)Attacks2.DashingSword2:
                        DashingSword2(player);
                        break;
                    case (int)Attacks2.OrbitingSwords2:
                        OrbitingSwords2(player);
                        break;
                    case (int)Attacks2.AxeSpin2:
                        AxeSpin2(player);
                        break;
                    case (int)Attacks.HammerStorm:
                        HammerStorm2(player);
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        void SwitchAttacks()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Attack++;
                if (Attack > 3)
                {
                    Attack = 0;
                }
                if (phase == 1) attackDuration = attackDurations[(int)Attack];
                else attackDuration = attackDurations2[(int)Attack];

                AttackCount = 0;
                AttackCount2 = 0;
                AttackTimer = 0;
                AttackTimer2 = 0;
                Swords.Clear();
                Axes.Clear();
                grow = true;
                rotSpeedMul = 1;
                foreach (var proj in Proj)
                {
                    if (proj.Key != "Sphere")
                        DeleteProjectiles(proj.Value);
                }
            }
            NPC.netUpdate = true;
        }

        public void DashingSword(Player player)
        {
            swordOffset = 200;
            if (AttackCount < 4)
            {
                switch (AttackTimer)
                {
                    case 0:
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            targetPosition = player.Center + new Vector2(Main.rand.Next(350, 500) * -Math.Sign(player.Center.X - NPC.Center.X), Main.rand.NextFloat(-100, 100));
                            NPC.netUpdate = true;
                        }
                        break;
                    case < 60:
                        if (rotSpeedMul > 0.04f)
                        {
                            rotSpeedMul -= 0.04f;
                        }
                        break;
                    case > 60 and < 120:
                        rotSpeedMul += 0.02f;
                        isDashing = true;
                        NPC.velocity = -playerDirection.SafeNormalize(Vector2.Zero) * 2;
                        break;
                    case 130:
                        tempPos = player.Center;
                        NPC.netUpdate = true;
                        break;
                    case 160:
                        NPC.velocity = NPC.Center.DirectionTo(tempPos).SafeNormalize(Vector2.Zero) * 50;
                        NPC.netUpdate = true;
                        rotSpeedMul = 8;
                        break;
                    case >= 200:
                        isDashing = false;
                        AttackTimer = -1;
                        NPC.velocity = Vector2.Zero;
                        AttackCount++;
                        NPC.netUpdate = true;
                        break;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                    {
                        Vector2 pos = NPC.Center + new Vector2(0, NPC.height / 2);
                        for (int i = 0; i < 4; i++)
                        {
                            Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 0, ai1: NPC.whoAmI));
                        }
                        NPC.netUpdate = true;
                    }
                }

                for (int i = 0; i < Swords.Count; i++)
                {
                    var sword = Swords[i];
                    Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer * rotSpeedMul)).RotatedBy(i * MathHelper.PiOver2);
                    sword.Center = NPC.Center + rotatedPos;
                    sword.timeLeft = 180;
                }

                if (!isDashing)
                {
                    NPC.velocity += new Vector2(0.1f * Math.Sign(playerDirection.X), 0.1f * Math.Sign(playerDirection.Y));
                }
            }
            else
            {
                NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 5;
                if (AttackTimer >= 30)
                {
                    AttackTimer = 0;
                    NPC.netUpdate = true;
                }
                if (AttackTimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash);
                    AttackCount2++;
                    Swords.Clear();
                    NPC.netUpdate = true;
                }

                if (AttackTimer > 10)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 pos = NPC.Center + new Vector2(0, swordOffset).RotatedBy(i * MathHelper.PiOver2).RotatedBy(MathHelper.ToRadians(15 * AttackCount2));
                                Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 1, ai2: 30));
                            }
                            NPC.netUpdate = true;
                        }
                    }
                }
            }
            AttackTimer++;
        }

        public void OrbitingSwords(Player player)
        {
            float XaccelMod = Math.Sign(playerDirection.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(playerDirection.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2((XaccelMod * 0.04f) + 0.02f * Math.Sign(playerDirection.X), (YaccelMod * 0.04f) + 0.02f * Math.Sign(playerDirection.Y));

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Swords.Count < 8 || Swords.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 pos = NPC.Center + new Vector2(0, NPC.height / 2);
                        Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 0, ai1: NPC.whoAmI));
                    }
                    NPC.netUpdate = true;
                }
            }

            for (int i = 0; i < Swords.Count; i++)
            {
                var sword = Swords[i];
                Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer)).RotatedBy(i * MathHelper.PiOver4);
                sword.Center = NPC.Center + rotatedPos;
                if (swordOffset <= 200)
                {
                    grow = true;
                    NPC.netUpdate = true;
                }
                if (swordOffset >= 1200)
                {
                    grow = false;
                    NPC.netUpdate = true;
                }

                if (grow) swordOffset++;
                else swordOffset--;
                sword.timeLeft = 180;
            }

            if (AttackTimer % 120 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), Proj["Hammer"], NPC.damage, 0, ai0: 60, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }
            AttackTimer++;
        }

        public void AxeSpin(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Axes.Count < 11 || Axes.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case <= 120:
                    for (int i = 0; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    break;
                case > 120:
                    for (int i = 0; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2));
                    }
                    if (AttackTimer2 % 120 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -5).RotatedBy(i * MathHelper.PiOver4), ProjectileID.CultistBossFireBall, NPC.damage, 1);
                            }
                        }
                    }
                    AttackTimer2++;
                    break;
            }
            AttackTimer++;
        }

        public void HammerStorm(Player player)
        {
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    targetPosition = NPC.Center + new Vector2(Main.rand.NextFloat(-arenaDistance, arenaDistance), Main.rand.NextFloat(-arenaDistance - 150, -arenaDistance + 150));
                    randomNum = Main.rand.NextFloat(arenaDistance - 1000, arenaDistance + 200);
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), targetPosition, new Vector2(0, 1), Proj["Hammer"], NPC.damage, 1, ai0: 60, ai1: targetPosition.X, ai2: targetPosition.Y + randomNum);
                }
                AttackTimer = 15;
            }
            AttackTimer--;
        }

        public void DashingSword2(Player player)
        {
            swordOffset = 200;
            if (AttackCount < 4)
            {
                switch (AttackTimer)
                {
                    case 0:
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            targetPosition = player.Center + new Vector2(Main.rand.Next(350, 500) * -Math.Sign(player.Center.X - NPC.Center.X), Main.rand.NextFloat(-100, 100));
                            NPC.netUpdate = true;
                        }
                        break;
                    case < 50:
                        if (rotSpeedMul > 0.04f)
                        {
                            rotSpeedMul -= 0.04f;
                        }
                        break;
                    case > 50 and < 100:
                        rotSpeedMul += 0.02f;
                        isDashing = true;
                        NPC.velocity = -playerDirection.SafeNormalize(Vector2.Zero) * 2;
                        break;
                    case 120:
                        tempPos = player.Center;
                        NPC.netUpdate = true;
                        break;
                    case 140:
                        NPC.velocity = NPC.Center.DirectionTo(tempPos).SafeNormalize(Vector2.Zero) * 50;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -10).RotatedBy(i * MathHelper.PiOver4), Proj["Fireball"], NPC.damage, 0);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -5).RotatedBy(i * MathHelper.PiOver4), Proj["Fireball"], NPC.damage, 0);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -2).RotatedBy(i * MathHelper.PiOver4), Proj["Fireball"], NPC.damage, 0);
                            }
                        }
                        rotSpeedMul = 8;
                        NPC.netUpdate = true;
                        break;
                    case >= 180:
                        isDashing = false;
                        AttackTimer = -1;
                        NPC.velocity = Vector2.Zero;
                        AttackCount++;
                        NPC.netUpdate = true;
                        break;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 pos = NPC.Center + new Vector2(0, NPC.height / 2);
                            Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 0, ai1: NPC.whoAmI));
                        }
                        NPC.netUpdate = true;
                    }
                }

                for (int i = 0; i < Swords.Count; i++)
                {
                    var sword = Swords[i];
                    Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer * rotSpeedMul)).RotatedBy(i * MathHelper.PiOver2);
                    sword.Center = NPC.Center + rotatedPos;
                    sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    sword.timeLeft = 180;
                }

                if (!isDashing)
                {
                    NPC.velocity += new Vector2(0.2f * Math.Sign(playerDirection.X), 0.2f * Math.Sign(playerDirection.Y));
                }
            }
            else
            {
                NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 5;
                if (AttackTimer >= 30)
                {
                    AttackTimer = 0;
                    NPC.netUpdate = true;
                }
                if (AttackTimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash);
                    AttackCount2++;
                    Swords.Clear();
                    NPC.netUpdate = true;
                }
                if (AttackTimer > 10)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (Swords.Count < 8 || Swords.Any(proj => proj.active == false))
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 pos = NPC.Center + new Vector2(0, swordOffset).RotatedBy(i * MathHelper.PiOver4).RotatedBy(MathHelper.ToRadians(15 * AttackCount2));
                                Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 1, ai2: 30));
                            }
                        }
                    }
                }
            }
            AttackTimer++;
        }

        public void OrbitingSwords2(Player player)
        {
            float XaccelMod = Math.Sign(playerDirection.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(playerDirection.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2((XaccelMod * 0.06f) + 0.02f * Math.Sign(playerDirection.X), (YaccelMod * 0.06f) + 0.02f * Math.Sign(playerDirection.Y));

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Swords.Count < 8 || Swords.Any(proj => proj.active == false))
                {
                    Vector2 pos = NPC.Center + new Vector2(0, NPC.height / 2);
                    Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, (pos - NPC.Center).SafeNormalize(Vector2.Zero), Proj["Sword"], NPC.damage, 1, ai0: 0, ai1: NPC.whoAmI));
                }
            }

            for (int i = 0; i < Swords.Count; i++)
            {
                var sword = Swords[i];
                Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer)).RotatedBy(i * MathHelper.PiOver4);
                sword.Center = NPC.Center + rotatedPos;
                if (swordOffset <= 200)
                {
                    grow = true;
                    NPC.netUpdate = true;
                }
                if (swordOffset >= 1400)
                {
                    grow = false;
                    NPC.netUpdate = true;
                }

                if (grow) swordOffset += 0.5f;
                else swordOffset -= 0.5f;
                sword.timeLeft = 180;
            }

            if (AttackTimer % 120 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), Proj["Hammer"], NPC.damage, 0, ai0: 60, ai1: player.Center.X, ai2: player.Center.Y);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), Proj["Hammer"], NPC.damage, 0, ai0: 30, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }
            AttackTimer++;
        }

        public void AxeSpin2(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Axes.Count < 11 || Axes.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
                else if (Axes.Count < 22 || Axes.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case <= 120:
                    for (int i = 0; i < Axes.Count / 2; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    for (int i = 11; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, ((i % 11) + 1) * 150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    break;
                case > 120:
                    for (int i = 0; i < Axes.Count / 2; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2 * (1 + AttackCount)));
                    }
                    for (int i = 11; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, ((i % 11) + 1) * 150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2 * (1 + AttackCount)));
                    }
                    if (AttackTimer2 % 90 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * (MathHelper.Pi / 16)) * 15, Proj["Fireball"], NPC.damage, 1);
                            }
                        }
                    }
                    if (AttackTimer2 > 120 && AttackTimer2 < 600)
                    {
                        if (arenaDistance < BaseArenaDistance + 500)
                        {
                            arenaDistance += 500f / 120f;
                        }
                    }
                    else if (AttackTimer2 >= 600)
                    {
                        AttackCount = MathHelper.Lerp(AttackCount, 0, 0.02f);
                        if (arenaDistance > BaseArenaDistance)
                        {
                            arenaDistance -= 500f / 150f;
                        }
                    }
                    if (AttackCount < 7 && AttackTimer2 < 600)
                        AttackCount += 0.001f;
                    AttackTimer2++;
                    break;
            }
            AttackTimer++;
        }

        public void HammerStorm2(Player player)
        {
            if (AttackTimer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 direction = new Vector2(0, 1).RotatedBy(AttackCount * MathHelper.PiOver4).RotatedBy(AttackCount2 * (Math.PI / 10));
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction, Proj["Hammer"], NPC.damage, 1, ai0: 45, ai1: NPC.Center.X + direction.X * 500, ai2: NPC.Center.Y + direction.Y * 500);
                }
                if (AttackCount < 8)
                {
                    AttackTimer = 8f;
                }
                else
                {
                    AttackTimer = 60;
                    AttackCount = 0;
                    AttackCount2++;
                }
                AttackCount++;
            }
            AttackTimer--;
        }

        const float BaseArenaDistance = 1800;
        public void Arena()
        {
            if (Spheres.Count < 40)
            {
                for (int i = 0; i < 40; i++)
                {
                    var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 60f);
                    Spheres.Add(sphere);
                }
                NPC.netUpdate = true;
            }

            for (int i = 0; i < Spheres.Count; i++)
            {
                Vector2 pos = NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)).RotatedBy(MathHelper.ToRadians(AITimer));
                Spheres[i].velocity = (pos - Spheres[i].position).SafeNormalize(Vector2.Zero) * (Spheres[i].position.Distance(pos) / 20);
                Spheres[i].timeLeft = 180;
            }
            if (arenaDistance < BaseArenaDistance)
            {
                arenaDistance += BaseArenaDistance / 60f;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.MountedCenter) > arenaDistance + 50)
                {
                    player.AddBuff(ModContent.BuffType<Infected>(), 2);
                }
            }
        }

        public void DeleteProjectiles(int projID)
        {
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == projID)
                {
                    proj.Kill();
                }
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override bool CheckDead()
        {
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("DivineSeekerShader");
            return true;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.65f);
            NPC.damage = (int)(NPC.damage * balance);
            NPC.defense = 30;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Parashard>(), 1, 20, 30));
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicFlames>(), 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<ParashardSword>(), ModContent.ItemType<ParacosmicFurnace>(), ModContent.ItemType<GravityBarrage>(), ModContent.ItemType<ParacosmicEyeStaff>()));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DivineSeekerBossBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.DivineSeekerRelic>()));
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }


        public override void OnKill()
        {
            DeleteProjectiles(Proj["Sphere"]);
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedDivineSeeker, -1);
        }
    }
}