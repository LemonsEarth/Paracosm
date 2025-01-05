using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Common.Utils;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.TheNameless
{
    [AutoloadBossHead]
    public class TheNameless : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        public float Attack
        {
            get { return NPC.ai[1]; }
            private set
            {
                int diffMod = -1; // One less attack if not in expert
                if (Main.expertMode)
                {
                    diffMod = 0;
                }
                int maxVal = phase == 1 ? 3 : 2;
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
        int phase { get; set; } = 1;
        bool phaseTransition = false;

        float attackDuration = 0;
        int[] attackDurations = { 480, 480, 900, 1200, 600 };
        int[] attackDurations2 = { 900, 900, 720, 900 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;
        Vector2 arenaCenter = Vector2.Zero;
        bool arenaFollow = true;

        List<Projectile> Spheres = new List<Projectile>();
        List<Projectile> VoidEruptionHooks = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()},
            {"SplitSpear", ModContent.ProjectileType<VoidSpearSplit>()},
            {"Bomb", ModContent.ProjectileType<VoidBombHostile>()},
            {"Vortex", ModContent.ProjectileType<VoidVortex>()},
            {"VoidEruption", ModContent.ProjectileType<VoidEruption>()},
            {"Fist", ModContent.ProjectileType<DarkFist>()},
        };

        public enum Attacks
        {
            SplitSpearThrow,
            RainingSplitShot,
            BombsWithSpear,
            VortexSpam,
        }

        public enum Attacks2
        {
            VoidEruptionSpin,
            DashFistSpam,
            BombRain,
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() { };
            /*{
                PortraitScale = 0.2f,
                PortraitPositionYOverride = -150
            };*/
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 50;
            NPC.height = 56;
            NPC.Opacity = 1;
            NPC.lifeMax = 1500000;
            NPC.defense = 40;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.value = 10000000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/AnotherSamePlace");
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.6f);
            NPC.damage = (int)(NPC.damage * balance * 0.5f);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPosition.X);
            writer.Write(targetPosition.Y);
            writer.Write(attackDuration);
            writer.Write(phase);
            writer.Write(phase2FirstTime);
            writer.Write(AttackTimer2);
            writer.Write(AttackCount2);
            writer.Write(arenaCenter.X);
            writer.Write(arenaCenter.Y);
            writer.Write(arenaFollow);
            writer.Write(Spheres.Count);
            writer.Write(phaseTransition);
            writer.Write(NPC.Opacity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPosition.X = reader.ReadSingle();
            targetPosition.Y = reader.ReadSingle();
            attackDuration = reader.ReadSingle();
            phase = reader.ReadInt32();
            phase2FirstTime = reader.ReadBoolean();
            AttackTimer2 = reader.ReadInt32();
            AttackCount2 = reader.ReadInt32();
            arenaCenter.X = reader.ReadSingle();
            arenaCenter.Y = reader.ReadSingle();
            arenaFollow = reader.ReadBoolean();
            int count = reader.ReadInt32();
            int sphereCounter = 0;
            Spheres.Clear();
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Proj["Sphere"])
                {
                    Spheres.Add(proj);
                    sphereCounter++;
                    if (sphereCounter >= count)
                    {
                        break;
                    }
                }
            }
            phaseTransition = reader.ReadBoolean();
            NPC.Opacity = reader.ReadSingle();
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }
            player = Main.player[NPC.target];
            playerDirection = player.Center - NPC.Center;

            DespawnCheck();

            //Visuals
            if (AITimer == 0)
            {
                NPC.Opacity = 0;
            }

            Visuals();

            if (AITimer > INTRO_DURATION - 150)
            {
                Arena();
            }

            if (AITimer < INTRO_DURATION)
            {
                Intro();
                AITimer++;
                return;
            }
            if (!phaseTransition)
            {
                NPC.dontTakeDamage = false;
            }

            if (NPC.life <= (NPC.lifeMax * 0.5f) && !phase2FirstTime)
            {
                SwitchAttacks();
                AttackTimer = PHASE_TRANSITION_DURATION;
                phaseTransition = true;
            }


            if (phaseTransition)
            {
                PhaseTransition();
                AITimer++;
                return;
            }

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            if (phase == 1)
            {
                NPC.defense = 60;
                switch (Attack)
                {
                    case (int)Attacks.SplitSpearThrow:
                        SplitSpearThrow();
                        break;
                    case (int)Attacks.RainingSplitShot:
                        RainingSplitShot();
                        break;
                    case (int)Attacks.BombsWithSpear:
                        BombsWithSpear();
                        break;
                    case (int)Attacks.VortexSpam:
                        VortexSpam();
                        break;
                }
            }
            else
            {
                NPC.defense = 100;
                switch (Attack)
                {
                    case (int)Attacks2.VoidEruptionSpin:
                        VoidEruptionSpin();
                        break;
                    case (int)Attacks2.DashFistSpam:
                        DashFistSpam();
                        break;
                    case (int)Attacks2.BombRain:
                        BombRain();
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        const int PHASE_TRANSITION_DURATION = 420;
        void PhaseTransition()
        {
            switch (AttackTimer)
            {
                case PHASE_TRANSITION_DURATION:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        phase2FirstTime = true;
                        phase = 2;
                    }
                    if (!Main.dedServ)
                    {
                        Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/AMemoryOfATime");
                    }
                    if (SkyManager.Instance["Paracosm:NamelessSky"] != null && !SkyManager.Instance["Paracosm:NamelessSky"].IsActive())
                    {
                        SkyManager.Instance.Activate("Paracosm:NamelessSky");
                    }
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                    NPC.dontTakeDamage = true;
                    break;

                case PHASE_TRANSITION_DURATION - 180:
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch);
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen);
                    SoundEngine.PlaySound(SoundID.Zombie105 with { Pitch = -1f, Volume = 2f });
                    break;

                case < PHASE_TRANSITION_DURATION - 180 and > PHASE_TRANSITION_DURATION - 360:
                    LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.Granite, Main.rand.NextFloat(1.2f, 1.8f), true);
                    LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.Granite, Main.rand.NextFloat(1.2f, 1.8f), true);
                    if (NPC.life < NPC.lifeMax)
                    {
                        int lifeHealed = NPC.lifeMax / 100;
                        if (NPC.life + lifeHealed > NPC.lifeMax)
                        {
                            lifeHealed = NPC.lifeMax - NPC.life;
                        }
                        NPC.life += lifeHealed;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.HealEffect(lifeHealed, true);
                            NPC.netUpdate = true;
                        }
                    }
                    break;

                case 0:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        phaseTransition = false;
                    }
                    SwitchAttacks();
                    NPC.netUpdate = true;
                    return;
            }
            AttackTimer--;
        }

        void DespawnCheck()
        {
            if (player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000)
            {
                SkyManager.Instance.Deactivate("Paracosm:NamelessSky");
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (phase == 2)
            {
                modifiers.FinalDamage *= 0.8f;
            }
        }

        void Visuals()
        {
            Lighting.AddLight(NPC.Center, 10, 10, 10);

            if (playerDirection.X != 0 && !playerDirection.HasNaNs())
            {
                NPC.spriteDirection = -Math.Sign(playerDirection.X);
            }

            if (phase == 2)
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:DarknessShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    ScreenShaderData shader = Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:DarknessShader").GetShader();
                    shader.Shader.Parameters["distance"].SetValue(0.8f);
                    shader.Shader.Parameters["maxGlow"].SetValue(1.1f);

                }
            }
        }

        const int INTRO_DURATION = 300;
        void Intro()
        {
            NPC.dontTakeDamage = true;
            NPC.velocity = NPC.Center.DirectionTo(player.Center + new Vector2(500, 0)) * (NPC.Center.Distance(player.Center + new Vector2(500, 0)) / 36);
            NPC.Opacity += 1f / 20f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
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
                NPC.Opacity = 1f;
                foreach (var proj in Proj)
                {
                    if (proj.Key != "Sphere")
                        DeleteProjectiles(proj.Value);
                }
                VoidEruptionHooks.Clear();
            }

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
            NPC.netUpdate = true;
        }

        void ThrowSplitSpear(int timeToFire, int splitInterval, int splitCount)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["SplitSpear"], NPC.damage, 1f, ai0: timeToFire, ai1: splitInterval, ai2: splitCount);
                var spear = (VoidSpearSplit)proj.ModProjectile;
                spear.NPCOwner = NPC.whoAmI;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, number: proj.whoAmI);
                }
            }
        }

        const int SST_ATTACK_RATE = 60;
        const int SST_SPLIT_INTERVAL = 10;
        const int SST_SPLIT_COUNT = 1;
        void SplitSpearThrow()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 4;
            switch (AttackTimer)
            {
                case SST_ATTACK_RATE:
                    ThrowSplitSpear(SST_ATTACK_RATE, SST_SPLIT_INTERVAL, SST_SPLIT_COUNT);
                    break;
                case 0:
                    AttackTimer = SST_ATTACK_RATE;
                    return;
            }
            AttackTimer--;
        }

        const int RSS_ATTACK_RATE1 = 120;
        const int RSS_ATTACK_RATE2 = 10;
        const int RSS_ATTACK_COUNT = 6;
        const int RSS_SPLIT_COUNT = 2;
        void RainingSplitShot()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
            switch (AttackTimer)
            {
                case 0:
                    if (AttackCount >= RSS_ATTACK_COUNT)
                    {
                        AttackTimer = RSS_ATTACK_RATE1;
                        AttackCount = 0;
                    }
                    else
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center - Vector2.UnitY * 400, Vector2.UnitY * 7, ModContent.ProjectileType<VoidBoltSplit>(), NPC.damage, 1f, ai0: 60, ai1: RSS_SPLIT_COUNT);
                        }
                        AttackCount++;
                        AttackTimer = RSS_ATTACK_RATE2;
                    }
                    return;
            }
            AttackTimer--;
        }

        const int BOMBS_CD = 120;
        void BombsWithSpear()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
            switch (AttackTimer)
            {
                case 0:
                    AttackTimer = BOMBS_CD;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 pos = player.Center + Main.rand.NextVector2Circular(300, 300);
                            int direction = -1;
                            if (pos.Y > player.Center.Y)
                            {
                                direction = 1;
                            }
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.UnitY * Main.rand.Next(10, 25) * direction, Proj["Bomb"], NPC.damage, 1f, ai0: 60, ai1: 8, ai2: -direction);
                        }
                    }
                    AttackCount++;
                    if (AttackCount == 3)
                    {
                        ThrowSplitSpear(60, 5, 0);
                        AttackCount = 0;
                    }
                    return;
            }
            AttackTimer--;
        }

        const int VORTEX_START_TIME = 240;
        const int VORTEX_INDICATOR_TIME = 180;
        const int VORTEX_SHOOT_TIME = 120;
        void VortexSpam()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
            switch (AttackTimer)
            {
                case VORTEX_INDICATOR_TIME:
                    LemonUtils.DustCircle(NPC.Center, 8, 15f, DustID.Granite, 1.3f);
                    break;
                case VORTEX_SHOOT_TIME:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero) * 15, Proj["Vortex"], NPC.damage, 1f, ai0: 300, ai1: 100, ai2: 30);
                    }
                    break;
                case 0:
                    AttackTimer = VORTEX_START_TIME;
                    return;
            }
            AttackTimer--;
        }

        const int ERUPTION_DEPLOY_TIME = 900;
        const int ERUPTION_SPEAR_SHOOT_INTERVAL = 60;
        const int ERUPTION_TIME_TO_POS = 60;
        const int ERUPTION_WAIT_TIME = 60;
        void VoidEruptionSpin()
        {
            switch (AttackTimer)
            {
                case ERUPTION_DEPLOY_TIME:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["VoidEruption"], NPC.damage, 1f, ai0: NPC.whoAmI, ai1: 30, ai2: 60);
                            VoidEruption eruption = proj.ModProjectile as VoidEruption;
                            Vector2 destination = NPC.Center + (Vector2.UnitY * ERUPTION_SPIN_ARENA_DISTANCE).RotatedBy(i * MathHelper.PiOver2);
                            eruption.Position = destination;
                            eruption.TimeToPosition = ERUPTION_TIME_TO_POS;
                            eruption.WaitTime = ERUPTION_WAIT_TIME;
                            eruption.RotSpeed = 1;
                            VoidEruptionHooks.Add(proj);
                            NetMessage.SendData(MessageID.SyncProjectile, number: proj.whoAmI);
                        }
                    }
                    break;
                case > 0:
                    if (AttackTimer % ERUPTION_SPEAR_SHOOT_INTERVAL == 0 && Main.expertMode)
                    {
                        ThrowSplitSpear(60, 20, 0);
                    }
                    break;
                case 0:
                    AttackTimer = ERUPTION_DEPLOY_TIME;
                    return;
            }
            NPC.velocity = Vector2.Zero;
            AttackTimer--;
        }

        const int DASH_FIST_START_TIME = 120;
        const int DASH_FIST_DASH_TIME = 60;
        const int DASH_FIST_FIST_TIME = 30;
        void DashFistSpam()
        {
            switch(AttackTimer)
            {
                case DASH_FIST_DASH_TIME:
                    NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 30;
                    break;
                case > DASH_FIST_FIST_TIME:
                    if (AITimer % 2 == 0)
                    {
                        LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.Granite, Main.rand.NextFloat(1.2f, 1.5f), true);
                    }
                    break;
                case DASH_FIST_FIST_TIME:
                    NPC.velocity = Vector2.Zero;

                    LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.Granite, Main.rand.NextFloat(1.5f, 2.3f), true);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -3; i <= 3; i++)
                        {
                            Vector2 direction = playerDirection.RotatedBy(MathHelper.ToRadians(i * 15 + Main.rand.Next(-10, 10)));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(5f, 8f), Proj["Fist"], NPC.damage, 1f, ai0: Main.rand.NextFloat(1.02f, 1.06f));
                        }
                    }
                    break;
                case > 0 and < DASH_FIST_FIST_TIME:
                    NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 8;
                    break;
                case 0:
                    AttackTimer = DASH_FIST_START_TIME;
                    return;
            }
            AttackTimer--;
        }

        const int BOMB_RAIN_START_TIME = 240;
        const int BOMB_RAIN_BOMB_TIME = 180;
        void BombRain()
        {
            switch (AttackTimer)
            {
                case BOMB_RAIN_START_TIME:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.velocity = Vector2.UnitY.RotatedByRandom(MathHelper.Pi * 2) * 10;
                    }
                    NPC.netUpdate = true;
                    break;
                case BOMB_RAIN_BOMB_TIME:
                    NPC.velocity = Vector2.Zero;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -1; i <= 1; i+=2)
                        {
                            Vector2 pos = player.Center + new Vector2(Main.rand.Next(-300, 300), i * 200);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, Proj["Bomb"], NPC.damage, 1f, ai0: 60, ai1: 8, ai2: -i * 1.4f);
                        }
                    }
                    break;
                case 0:
                    AttackTimer = BOMB_RAIN_START_TIME;
                    return;
            }
            AttackTimer--;
        }

        void MoveToPos(Vector2 pos, float xAccel = 1f, float yAccel = 1f, float xSpeed = 1f, float ySpeed = 1f)
        {
            Vector2 direction = NPC.Center.DirectionTo(pos);
            if (direction.HasNaNs())
            {
                return;
            }
            float XaccelMod = Math.Sign(direction.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(direction.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2(XaccelMod * xAccel + xSpeed * Math.Sign(direction.X), YaccelMod * yAccel + ySpeed * Math.Sign(direction.Y));
        }

        const int BASE_ARENA_DISTANCE = 1500;
        const int ERUPTION_SPIN_ARENA_DISTANCE = 800;
        const int DASHFIST_ARENA_DISTANCE = 1600;
        const int BOMBRAIN_ARENA_DISTANCE = 1200;
        public void Arena()
        {
            float targetArenaDistance = BASE_ARENA_DISTANCE;
            arenaFollow = true;

            if (phase == 1)
            {
                /*switch (Attack)
                {
                    case (int)Attacks.FlameBeamCombo:
                        targetArenaDistance = FB_COMBO_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                }*/
            }
            else
            {
                switch (Attack)
                {
                    case (int)Attacks2.VoidEruptionSpin:
                        targetArenaDistance = ERUPTION_SPIN_ARENA_DISTANCE;
                        arenaFollow = true;
                        break;
                    case (int)Attacks2.DashFistSpam:
                        targetArenaDistance = DASHFIST_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                    case (int)Attacks2.BombRain:
                        targetArenaDistance = BOMBRAIN_ARENA_DISTANCE;
                        arenaFollow = true;
                        break;
                }
            }
            if (AITimer % 5 == 0)
            {
                NPC.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Spheres.Count < 40)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 90f);

                        Spheres.Add(sphere);
                    }
                }
            }
            NPC.netUpdate = true;

            if (arenaFollow)
            {
                arenaCenter = NPC.Center;
            }

            for (int i = 0; i < Spheres.Count; i++)
            {
                Vector2 pos = arenaCenter + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)).RotatedBy(MathHelper.ToRadians(AITimer));
                if (Spheres[i].type != Proj["Sphere"])
                {
                    continue;
                }

                Spheres[i].velocity = (pos - Spheres[i].Center).SafeNormalize(Vector2.Zero) * (Spheres[i].Center.Distance(pos) / 50);
                Spheres[i].timeLeft = 180;
            }

            arenaDistance += ((targetArenaDistance - arenaDistance) / 60);

            foreach (var player in Main.ActivePlayers)
            {
                if (arenaCenter.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > INTRO_DURATION + 30)
                {
                    player.AddBuff(ModContent.BuffType<Infected>(), 2);
                }

                if (arenaCenter.Distance(player.MountedCenter) < arenaDistance + 50 && AITimer > INTRO_DURATION - 60)
                {
                    player.AddBuff(ModContent.BuffType<NebulousPower>(), 2);
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

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 6;
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

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (VoidEruptionHooks.Count == 0)
            {
                return;
            }
            foreach (var hook in VoidEruptionHooks) // draw hook chains
            {
                Vector2 drawPos = NPC.Center;
                Vector2 NPCToProj = hook.Center - NPC.Center;
                int segmentHeight = 34;
                float distanceLeft = NPCToProj.Length() + segmentHeight / 2;
                float rotation = NPCToProj.ToRotation();
                Texture2D texture = TextureAssets.Projectile[hook.type].Value;
                Rectangle secondFrame = texture.Frame(1, 3, 0, 1);

                while (distanceLeft > 0f)
                {
                    drawPos += NPCToProj.SafeNormalize(Vector2.Zero) * segmentHeight;
                    distanceLeft = drawPos.Distance(hook.Center);
                    distanceLeft -= segmentHeight;
                    Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, secondFrame, Color.White, rotation, new Vector2(17, 17), 1f, SpriteEffects.None);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<UnstableNebulousFlame>(), 1, 4, 8));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.FragmentNebula, 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NebulaMasterBossBag>()));
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
            SkyManager.Instance.Deactivate("Paracosm:NamelessSky");
            DeleteProjectiles(Proj["Sphere"]);
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedTheNameless, -1);
            for (int i = 0; i < 16; i++)
            {
                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(0, NPC.width), Main.rand.Next(0, NPC.height)), new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(2f, 5f));
            }
        }

        /*public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (phase == 1)
            {
                return true;
            }

            Asset<Texture2D> textureAsset = ModContent.Request<Texture2D>("Paracosm/Assets/Textures/Boss/NebulaMasterTrail");
            Texture2D texture = textureAsset.Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            for (int k = NPC.oldPos.Length - 1; k >= 0; k--)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin;
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                float scale = 1f;
                if (NPC.oldPos.Length - k > 0)
                {
                    float posMod = 1f / (NPC.oldPos.Length - k);
                    scale = ((float)Math.Sin(MathHelper.ToRadians(AITimer)) + 1) * 0.5f + posMod;
                }
                Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, drawOrigin, scale, spriteEffects, 0);
            }
            return true;
        }*/
    }
}