using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.SolarChampion
{
    [AutoloadBossHead]
    public class SolarChampion : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];

        float Attack
        {
            get { return NPC.ai[1]; }
            set
            {
                int diffMod = -1; // One less attack if not in expert
                if (Main.expertMode)
                {
                    diffMod = 0;
                }
                int maxVal = 3;
                if (value > maxVal + diffMod || value < 0)
                {
                    NPC.ai[1] = 0;
                }
                else
                {
                    NPC.ai[1] = value;
                }
            }
        }
        ref float AttackTimer => ref NPC.ai[2];
        ref float AttackCount => ref NPC.ai[3];

        int AttackTimer2 = 0;
        int AttackCount2 = 0;

        bool phase2FirstTime = false;
        int phase = 1;

        float attackDuration = 0;
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
            {"Sphere", ModContent.ProjectileType<BorderSphere>()}
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
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.SolarPillar,
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 106;
            NPC.height = 106;
            NPC.Opacity = 1;
            NPC.lifeMax = 600000;
            NPC.defense = 30;
            NPC.damage = 20;
            NPC.HitSound = SoundID.NPCHit57;
            NPC.DeathSound = SoundID.NPCHit57;
            NPC.value = 5000000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/CelestialShowdown");
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
            playerDirection = player.Center - NPC.Center;
            if (player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000)
            {
                NPC.active = false;
            }

            //Visuals
            if (AITimer == 0)
            {
                NPC.Opacity = 0;
            }
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IchorTorch);
            if (NPC.velocity.Length() > 10)
            {
                NPC.rotation = NPC.rotation.AngleLerp(playerDirection.X * MathHelper.ToRadians(30), MathHelper.ToRadians(1));
            }
            else
            {
                NPC.rotation = NPC.rotation.AngleLerp(0, MathHelper.ToRadians(3));
            }

            if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:ScreenTintShader").GetShader().UseColor(new Color(1.0f, 0.8f, 0.4f));
            }

            foreach (var p in Main.player)
            {
                p.solarMonolithShader = true;
            }

            Arena();

            if (AITimer < INTRO_DURATION)
            {
                Intro();
                AITimer++;
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.life <= NPC.lifeMax / 2 && !phase2FirstTime)
            {
                phase2FirstTime = true;
                phase = 2;
                SwitchAttacks();
                NPC.netUpdate = true;
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

        const int INTRO_DURATION = 60;
        void Intro()
        {
            NPC.dontTakeDamage = true;
            NPC.velocity = new Vector2(0, -2);
            NPC.Opacity += 1f / 60f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
            Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].GetShader().UseProgress(AITimer / 60);
        }

        void SwitchAttacks()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Attack++;
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

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
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
                    case > 60 and < 120: // Sling
                        rotSpeedMul += 0.02f;
                        isDashing = true;
                        NPC.velocity = -playerDirection.SafeNormalize(Vector2.Zero) * 2;
                        break;
                    case 130: // Set target as old player location
                        tempPos = player.Center;
                        NPC.netUpdate = true;
                        break;
                    case 160: // Charge
                        NPC.velocity = NPC.Center.DirectionTo(tempPos).SafeNormalize(Vector2.Zero) * 50;
                        NPC.netUpdate = true;
                        rotSpeedMul = 8;
                        break;
                    case >= 200: // Stop charging
                        isDashing = false;
                        AttackTimer = -1;
                        NPC.velocity = Vector2.Zero;
                        AttackCount++;
                        NPC.netUpdate = true;
                        break;
                    case > 160: // Create projs
                        if (AttackTimer % 10 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection, Proj["Fireball"], NPC.damage, 0, ai1: 2f);
                        }
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
            NPC.velocity += new Vector2(XaccelMod * 0.04f + 0.02f * Math.Sign(playerDirection.X), YaccelMod * 0.04f + 0.02f * Math.Sign(playerDirection.Y));

            if (Main.netMode != NetmodeID.MultiplayerClient) // Creating swords
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

            for (int i = 0; i < Swords.Count; i++) // Rotating swords
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

            if (AttackTimer % 120 == 0) // Firing hammers
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY, Proj["Hammer"], NPC.damage, 0, ai0: 60, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }

            if (attackDuration == 30) // Indicator for axes - next attack
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i += 2)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY * i, ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 13);
                    }
                }
            }
            AttackTimer++;
        }

        public void AxeSpin(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) // Creating axes
            {
                if (Axes.Count < 11 || Axes.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY, Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case 0:
                    SoundEngine.PlaySound(SoundID.Item71);
                    break;
                case <= 120: // Lerp axes to position
                    for (int i = 0; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    break;
                case > 120: // Speen
                    for (int i = 0; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2));
                    }
                    if (AttackTimer2 % 120 == 0) // Create projs
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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), targetPosition, Vector2.UnitY, Proj["Hammer"], NPC.damage, 1, ai0: 60, ai1: targetPosition.X, ai2: targetPosition.Y + randomNum);
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
                    case > 140:
                        if (AttackTimer % 6 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection, Proj["Fireball"], NPC.damage, 0, ai1: 3f);
                        }
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
            NPC.velocity += new Vector2(XaccelMod * 0.06f + 0.02f * Math.Sign(playerDirection.X), YaccelMod * 0.06f + 0.02f * Math.Sign(playerDirection.Y));

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
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY, Proj["Hammer"], NPC.damage, 0, ai0: 60, ai1: player.Center.X, ai2: player.Center.Y);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY, Proj["Hammer"], NPC.damage, 0, ai0: 30, ai1: player.Center.X, ai2: player.Center.Y);
                }
            }
            if (attackDuration == 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i += 2)
                    {
                        Vector2 indicatorDistancePos = NPC.Center + new Vector2(0, i * arenaDistance);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY * i, ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 12);
                    }
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
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitY, Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
                else if (Axes.Count < 22 || Axes.Any(proj => proj.active == false))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        var axe = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY, Proj["Axe"], NPC.damage, 0, ai1: NPC.whoAmI);
                        axe.timeLeft = 1200;
                        Axes.Add(axe);
                    }
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity /= 1.2f;
            switch (AttackTimer)
            {
                case 0:
                    SoundEngine.PlaySound(SoundID.Item71);
                    break;
                case <= 120:
                    AttackCount2 = 90;
                    for (int i = 0; i < Axes.Count / 2; i++)
                    {
                        Vector2 pos = new Vector2(0, (i + 1) * -150);
                        Axes[i].Center = Vector2.Lerp(Axes[i].Center, NPC.Center + pos, AttackTimer / 120f);
                    }
                    for (int i = 11; i < Axes.Count; i++)
                    {
                        Vector2 pos = new Vector2(0, (i % 11 + 1) * 150);
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
                        Vector2 pos = new Vector2(0, (i % 11 + 1) * 150);
                        Axes[i].Center = NPC.Center + pos.RotatedBy(MathHelper.ToRadians(-AttackTimer2 * (1 + AttackCount)));
                    }
                    if (AttackTimer2 % AttackCount2 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * (MathHelper.Pi / 8)) * 15, Proj["Fireball"], NPC.damage, 1);
                            }
                        }
                    }
                    if (AttackTimer2 > 120 && AttackTimer2 < 600)
                    {
                        if (arenaDistance < BaseArenaDistance + 500)
                        {
                            arenaDistance += 500f / 120f;
                        }
                        AttackCount2 = 75;
                    }
                    else if (AttackTimer2 >= 600)
                    {
                        AttackCount = MathHelper.Lerp(AttackCount, 0, 0.02f);
                        if (arenaDistance > BaseArenaDistance)
                        {
                            arenaDistance -= 500f / 150f;
                        }
                        AttackCount2 = 90;
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
                    Vector2 direction = Vector2.UnitY.RotatedBy(AttackCount * MathHelper.PiOver4).RotatedBy(AttackCount2 * (Math.PI / 10));
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
            if (Main.netMode != NetmodeID.MultiplayerClient)
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
            }

            for (int i = 0; i < Spheres.Count; i++)
            {
                Vector2 pos = NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)).RotatedBy(MathHelper.ToRadians(AITimer));

                Spheres[i].velocity = (pos - Spheres[i].position).SafeNormalize(Vector2.Zero) * (Spheres[i].position.Distance(pos) / 20);
                Spheres[i].timeLeft = 180;


            }
            if (arenaDistance < BaseArenaDistance)
            {
                arenaDistance += BaseArenaDistance / INTRO_DURATION;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > INTRO_DURATION * 2)
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
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("Paracosm:ScreenTintShader");
            return true;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            if (NPC.life <= 0)
            {
                int goreType1 = Mod.Find<ModGore>("SolarChampion_Gore1").Type;
                int goreType2 = Mod.Find<ModGore>("SolarChampion_Gore2").Type;

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), goreType1);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), goreType2);
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.65f);
            NPC.damage = (int)(NPC.damage * balance);
            NPC.defense = 50;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 8;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<SolarBurn>(), 180);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SolarCore>(), 1, 4, 8));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.FragmentSolar, 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SolarChampionBossBag>()));
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void OnKill()
        {
            DeleteProjectiles(Proj["Sphere"]);
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedSolarChampion, -1);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle drawRect = texture.Frame(1, Main.npcFrameCount[Type], 0, 0);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            for (int k = NPC.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRect, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}