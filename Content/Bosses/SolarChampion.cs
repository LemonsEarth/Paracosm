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
        int[] attackDurations = { 1200, 900, 1200, 900, 1200 };
        int[] attackDurations2 = { 1200, 900, 1200, 900, 1200 };
        List<Projectile> Spheres = new List<Projectile>();

        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sword", ModContent.ProjectileType<SolarBlade>()},
            {"Hammer", ModContent.ProjectileType<SolarHammer>()},
            {"Axe", ModContent.ProjectileType<SolarAxe>()},
            {"Fireball", ModContent.ProjectileType<SolarFireball>()},
            {"Sphere", ModContent.ProjectileType<SolarSphere>()}
        };

        Player player;
        Vector2 playerDirection;

        enum Attacks
        {
            DashingSword,
            OrbitingSwords,
            AxeSpin
        }

        enum Attacks2
        {
            DashingSword2,
            OrbitingSwords2,
            AxeSpin2
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
            NPC.lifeMax = 300000;
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

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];
            player.solarMonolithShader = true;
            playerDirection = (player.Center - NPC.Center);

            if (player.dead || NPC.Center.Distance(player.MountedCenter) > 5000)
            {
                NPC.EncourageDespawn(300);
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
                    case 3:
                        OrbitingSwords(player);
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
                    case 3:
                        OrbitingSwords2(player);
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        void SwitchAttacks()
        {
            Attack++;
            if (Attack > 3)
            {
                Attack = 0;
            }
            if (phase == 1) attackDuration = attackDurations[(int)Attack];
            else attackDuration = attackDurations2[(int)Attack];

            AttackCount = 0;
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

        Vector2 tempPos = Vector2.Zero;
        Vector2 leftOffset = new Vector2(-500, 0);
        Vector2 targetPosition = Vector2.Zero;
        float swordOffset = 200;
        float rotSpeedMul = 1;
        bool isDashing = false;
        List<Projectile> Swords = new List<Projectile>();
        List<Projectile> Axes = new List<Projectile>();


        public void DashingSword(Player player)
        {
            swordOffset = 200;
            if (AttackCount < 4)
            {
                switch (AttackTimer)
                {
                    case 0:
                        targetPosition = player.Center + new Vector2(Main.rand.Next(350, 500) * -Math.Sign(player.Center.X - NPC.Center.X), Main.rand.NextFloat(-100, 100));
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
                        break;
                    case 160:
                        NPC.velocity = NPC.Center.DirectionTo(tempPos).SafeNormalize(Vector2.Zero) * 50;
                        rotSpeedMul = 8;
                        break;
                    case >= 200:
                        isDashing = false;
                        AttackTimer = -1;
                        NPC.velocity = Vector2.Zero;
                        AttackCount++;
                        break;
                }
                if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                {
                    SummonSword();
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
                    NPC.velocity += new Vector2(0.1f * Math.Sign(playerDirection.X), 0.1f * Math.Sign(playerDirection.Y));
                }
            }
            else
            {
                NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 5;
                if (AttackTimer >= 30)
                {
                    AttackTimer = 0;
                }
                if (AttackTimer == 0)
                {
                    foreach (var sword in Swords)
                    {
                        sword.velocity *= 20;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash);
                    AttackCount2++;
                    Swords.Clear();
                }
                if (AttackTimer > 10)
                {
                    if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                    {
                        SummonSword();
                        for (int i = 0; i < Swords.Count; i++)
                        {
                            var sword = Swords[i];
                            Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(i * MathHelper.PiOver2).RotatedBy(MathHelper.ToRadians(15 * AttackCount2));
                            sword.Center = NPC.Center + rotatedPos;
                            sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
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

            if (Swords.Count < 8 || Swords.Any(proj => proj.active == false))
            {
                SummonSword();
            }

            for (int i = 0; i < Swords.Count; i++)
            {
                var sword = Swords[i];
                Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer)).RotatedBy(i * MathHelper.PiOver4);
                sword.Center = NPC.Center + rotatedPos;
                if (swordOffset <= 200)
                {
                    grow = true;
                }
                if (swordOffset >= 1200)
                {
                    grow = false;
                }

                if (grow) swordOffset++;
                else swordOffset--;
                sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                sword.timeLeft = 180;
            }

            if (AttackTimer % 120 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero) * 10, Proj["Hammer"], NPC.damage, 0, ai0: 1, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }
            AttackTimer++;
        }

        public void AxeSpin(Player player)
        {
            if (Axes.Count < 11 || Axes.Any(proj => proj.active == false))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Axes.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI));
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case <= 120:
                    for (int i = 0; i < Axes.Count(); i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    break;
                case > 120:
                    for (int i = 0; i < Axes.Count(); i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2));
                        Axes[i].velocity = (Axes[i].Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        Axes[i].timeLeft = 30;
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

        public void DashingSword2(Player player)
        {
            swordOffset = 200;
            if (AttackCount < 4)
            {
                switch (AttackTimer)
                {
                    case 0:
                        targetPosition = player.Center + new Vector2(Main.rand.Next(350, 500) * -Math.Sign(player.Center.X - NPC.Center.X), Main.rand.NextFloat(-100, 100));
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
                        break;
                    case >= 180:
                        isDashing = false;
                        AttackTimer = -1;
                        NPC.velocity = Vector2.Zero;
                        AttackCount++;
                        break;
                }
                if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                {
                    SummonSword();
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
                }
                if (AttackTimer == 0)
                {
                    foreach (var sword in Swords)
                    {
                        sword.velocity *= 15;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash);
                    AttackCount2++;
                    Swords.Clear();
                }
                if (AttackTimer > 10)
                {
                    if (Swords.Count < 8)
                    {
                        SummonSword();
                        for (int i = 0; i < Swords.Count; i++)
                        {
                            var sword = Swords[i];
                            Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(i * MathHelper.PiOver4).RotatedBy(MathHelper.ToRadians(15 * AttackCount2));
                            sword.Center = NPC.Center + rotatedPos;
                            sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        }
                    }
                }
            }
            AttackTimer++;
        }

        bool grow = true;
        public void OrbitingSwords2(Player player)
        {
            float XaccelMod = Math.Sign(playerDirection.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(playerDirection.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2((XaccelMod * 0.06f) + 0.02f * Math.Sign(playerDirection.X), (YaccelMod * 0.06f) + 0.02f * Math.Sign(playerDirection.Y));

            if (Swords.Count < 8 || Swords.Any(proj => proj.active == false))
            {
                SummonSword();
            }

            for (int i = 0; i < Swords.Count; i++)
            {
                var sword = Swords[i];
                Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer)).RotatedBy(i * MathHelper.PiOver4);
                sword.Center = NPC.Center + rotatedPos;
                if (swordOffset <= 200)
                {
                    grow = true;
                }
                if (swordOffset >= 1400)
                {
                    grow = false;
                }

                if (grow) swordOffset += 0.5f;
                else swordOffset -= 0.5f;
                sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                sword.timeLeft = 180;
            }

            if (AttackTimer % 120 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero) * 10, Proj["Hammer"], NPC.damage, 0, ai0: 1, ai1: player.Center.X, ai2: player.Center.Y);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero) * 20, Proj["Hammer"], NPC.damage, 0, ai0: 1, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }
            AttackTimer++;
        }

        public void AxeSpin2(Player player)
        {
            if (Axes.Count < 11 || Axes.Any(proj => proj.active == false))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Axes.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI));
                }
            }
            else if (Axes.Count < 22 || Axes.Any(proj => proj.active == false))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Axes.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI));
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case <= 120:
                    for (int i = 0; i < Axes.Count() / 2; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    for (int i = 11; i < Axes.Count(); i++)
                    {
                        Vector2 pos = new Vector2(0, ((i % 11) + 1) * 150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    break;
                case > 120:
                    for (int i = 0; i < Axes.Count() / 2; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2 * (1 + AttackCount)));
                        Axes[i].timeLeft = 30;
                    }
                    for (int i = 11; i < Axes.Count(); i++)
                    {
                        Vector2 pos = new Vector2(0, ((i % 11) + 1) * 150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2 * (1 + AttackCount)));
                        Axes[i].timeLeft = 30;
                    }
                    if (AttackTimer2 % 60 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (AttackTimer < 120)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver4), ProjectileID.CultistBossFireBall, NPC.damage, 1);
                                }
                            }
                            else
                            {
                                for (int i = -1; i <= 1; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * (MathHelper.Pi / 8)) * 15, Proj["Fireball"], NPC.damage, 1);
                                }
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
                        if (AttackCount > 0.25)
                        {
                            AttackCount -= 0.05f;
                        }
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

        void SummonSword(int count = 1)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center + new Vector2(0, NPC.height / 2).RotatedBy(MathHelper.ToRadians(i * (360 / count))),
                    Vector2.Zero,
                    Proj["Sword"],
                    NPC.damage,
                    1));
            }
        }

        float arenaDistance = 0;
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
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedDivineSeeker, -1);
        }
    }
}