using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Common.Utils;
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
using Terraria.Chat;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.NebulaMaster
{
    [AutoloadBossHead]
    public class NebulaMaster : ModNPC
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
                int maxVal = phase == 1 ? 2 : 3;
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
        int[] attackDurations = { 480, 450, 900 };
        int[] attackDurations2 = { 600, 900, 720, 900 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;
        Vector2 arenaCenter = Vector2.Zero;
        bool arenaFollow = true;
        bool spawnedAura = false;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()},
            {"SpeedFlames",  ModContent.ProjectileType<SpeedyNebulousFlames>()},
            {"Aura", ModContent.ProjectileType<NebulousAuraHostile>()},
            {"Beam", ModContent.ProjectileType<NebulaBeam>()}
        };

        public enum Attacks
        {
            SpeedingFlames,
            RapidDashes,
            FlameBeamCombo
        }

        public enum Attacks2
        {
            BoxingRing,
            DashAuraSpam,
            LaserSpam,
            DashBlaster
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            Main.npcFrameCount[NPC.type] = 9;
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.NebulaPillar,
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 86;
            NPC.height = 154;
            NPC.Opacity = 1;
            NPC.lifeMax = 480000;
            NPC.defense = 40;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCHit52;
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
            writer.Write(spawnedAura);
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
            spawnedAura = reader.ReadBoolean();
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
            if ((player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000))
            {
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }

            //Visuals
            if (AITimer == 0)
            {
                NPC.Opacity = 0;
            } 

            Lighting.AddLight(NPC.Center, 100, 100, 100);

            if (playerDirection.X != 0 && !playerDirection.HasNaNs())
            {
                NPC.spriteDirection = -Math.Sign(playerDirection.X);
            }

            foreach (var p in Main.player)
            {
                p.nebulaMonolithShader = true;
            }

            if (AITimer > INTRO_DURATION - 60)
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
            float rotmul = NPC.spriteDirection == 1 ? MathHelper.Pi : 0;
            NPC.rotation = playerDirection.ToRotation() + rotmul;

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
                switch (Attack)
                {
                    case (int)Attacks.SpeedingFlames:
                        SpeedingFlames();
                        break;
                    case (int)Attacks.RapidDashes:
                        RapidDashes();
                        break;
                    case (int)Attacks.FlameBeamCombo:
                        FlameBeamCombo();
                        break;
                }
            }
            else
            {
                NPC.defense = NPC.defDefense + 60;
                for (int i = 0; i < 2; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemDiamond);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemAmethyst);
                }
                switch (Attack)
                {
                    case (int)Attacks2.BoxingRing:
                        BoxingRing();
                        break;
                    case (int)Attacks2.DashAuraSpam:
                        DashAuraSpam();
                        break;
                    case (int)Attacks2.LaserSpam:
                        LaserSpam();
                        break;
                    case (int)Attacks2.DashBlaster:
                        DashBlaster();
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        const int INTRO_DURATION = 180;
        void Intro()
        {
            if (AITimer == INTRO_DURATION - 90)
            {
                DialogueMessage("Intro", 5);
            }
            NPC.dontTakeDamage = true;
            NPC.velocity = NPC.Center.DirectionTo(player.Center + new Vector2(500, 0)) * (NPC.Center.Distance(player.Center + new Vector2(500, 0)) / 12);
            NPC.Opacity += 1f / 20f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
        }

        const int PHASE_TRANSITION_DURATION = 480;
        void PhaseTransition()
        {
            NPC.Opacity = 1f;
            switch (AttackTimer)
            {
                case PHASE_TRANSITION_DURATION:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        phase2FirstTime = true;
                        phase = 2;
                    }
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                    NPC.dontTakeDamage = true;
                    break;

                case PHASE_TRANSITION_DURATION - 90:
                    DialogueMessage("PhaseTransitionStart", 5, 90);
                    break;

                case PHASE_TRANSITION_DURATION - 180:
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch);
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen);
                    SoundEngine.PlaySound(SoundID.Zombie105);
                    break;

                case < PHASE_TRANSITION_DURATION - 180 and > PHASE_TRANSITION_DURATION - 360:
                    LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.GemDiamond, Main.rand.NextFloat(1.2f, 1.8f), true);
                    LemonUtils.DustCircle(NPC.Center, 16, Main.rand.NextFloat(15, 20), DustID.GemAmethyst, Main.rand.NextFloat(1.2f, 1.8f), true);
                    if (NPC.life < NPC.lifeMax)
                    {
                        int lifeHealed = 5000;
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

                case PHASE_TRANSITION_DURATION - 360:
                    DialogueMessage("PhaseTransitionEnd", 5, 90);
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
                spawnedAura = false;
                NPC.Opacity = 1f;
                if (phase == 1 && Attack == (int)Attacks.RapidDashes)
                {
                    AttackTimer = DASH_START_COOLDOWN;
                    NPC.Opacity = 0f;
                }
                if (phase == 2)
                {
                    if (Attack == (int)Attacks2.BoxingRing)
                    {
                        AttackTimer = BOXING_RING_START_TIME;
                    }
                    else if (Attack == (int)Attacks2.DashAuraSpam)
                    {
                        AttackTimer = DASH2_COOLDOWN;
                        NPC.Opacity = 0f;
                    }
                    else if (Attack == (int)Attacks2.DashBlaster)
                    {
                        AttackTimer = DASH_BLASTER_START_TIME;
                    }
                }
                foreach (var proj in Proj)
                {
                    if (proj.Key != "Sphere")
                        DeleteProjectiles(proj.Value);
                }
            }

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
            NPC.netUpdate = true;
        }

        const int RANDOM_POS_TIME = 60;
        const int MOVE_TIME = 30;
        const int ATTACK_RATE = 15;
        void SpeedingFlames()
        {
            switch (AttackTimer)
            {
                case RANDOM_POS_TIME:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        targetPosition = NPC.Center + new Vector2(Main.rand.NextFloat(-300, 300), Main.rand.NextFloat(-300, 300));
                    }
                    NPC.netUpdate = true;
                    break;
                case > MOVE_TIME:
                    MoveToPos(targetPosition, 1, 1, 2f, 2f);
                    break;
                case > 0:
                    if (AttackTimer % ATTACK_RATE == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Vector2 direction = playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(i * 15));
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 2, Proj["SpeedFlames"], NPC.damage / 2, 1, ai0: 30, ai1: 25, ai2: 1);
                            }
                        }
                    }
                    break;
                case 0:
                    AttackTimer = RANDOM_POS_TIME;
                    return;
            }

            AttackTimer--;
        }

        const int DASH_START_COOLDOWN = 120;
        const int DASH_COOLDOWN = 35;
        const int DASH_SPEED = 40;
        void RapidDashes()
        {
            switch (AttackTimer)
            {
                case > DASH_COOLDOWN and < DASH_START_COOLDOWN:
                    if (!spawnedAura)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["Aura"], NPC.damage, 1, ai1: NPC.whoAmI);
                        spawnedAura = true;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GemAmethyst);
                        dust.noGravity = true;
                        dust.velocity = new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));
                    }
                    break;
                case DASH_COOLDOWN:
                    targetPosition = playerDirection;
                    break;
                case > 0 and < DASH_COOLDOWN:
                    NPC.velocity = targetPosition.SafeNormalize(Vector2.Zero) * DASH_SPEED;
                    break;
                case <= 0:
                    AttackTimer = DASH_COOLDOWN;
                    return;
            }
            AttackTimer--;
        }

        const int FB_COMBO_POS_COOLDOWN = 180;
        const int FB_COMBO_START_TIME = 120;
        const int FB_COMBO_FLAME_COOLDOWN = 5;
        const int FB_COMBO_MAX_FLAME_COUNT = 8;
        const int FB_COMBO_LASER_POS_TIME = 60;
        const int FB_COMBO_LASER_TIME = 30;
        void FlameBeamCombo()
        {
            for (int i = 0; i < 64; i++)
            {
                Vector2 dustPos = arenaCenter + (Vector2.UnitY * FB_COMBO_ARENA_DISTANCE * 0.75f).RotatedBy(MathHelper.ToRadians(i * 360f / 64f));
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.GemDiamond);
                dust.noGravity = true;
            }
            switch (AttackTimer)
            {
                case FB_COMBO_POS_COOLDOWN:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        targetPosition = arenaCenter + new Vector2(Main.rand.NextFloat(-FB_COMBO_ARENA_DISTANCE * 0.75f, FB_COMBO_ARENA_DISTANCE * 0.75f), Main.rand.NextFloat(-FB_COMBO_ARENA_DISTANCE * 0.75f, FB_COMBO_ARENA_DISTANCE * 0.75f));
                    }
                    NPC.netUpdate = true;
                    break;
                case > FB_COMBO_START_TIME:
                    MoveToPos(targetPosition, 1.2f, 1.2f, 2, 2);
                    break;
                case 0:
                    AttackTimer = FB_COMBO_POS_COOLDOWN;
                    AttackCount = 0;
                    return;
                case <= FB_COMBO_START_TIME:
                    NPC.velocity = Vector2.Zero;
                    if (AttackTimer % FB_COMBO_FLAME_COOLDOWN == 0 && AttackCount < FB_COMBO_MAX_FLAME_COUNT)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 direction = playerDirection.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(AttackCount * 15));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 2, Proj["SpeedFlames"], NPC.damage / 2, 1, ai0: 30, ai1: 25, ai2: 1);
                        }
                        AttackCount++;
                    }
                    if (AttackTimer == FB_COMBO_LASER_POS_TIME)
                    {
                        targetPosition = playerDirection;
                    }
                    if (AttackTimer == FB_COMBO_LASER_TIME)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, targetPosition.SafeNormalize(Vector2.Zero), Proj["Beam"], NPC.damage * 2, 1, ai0: 100, ai1: 0);
                        }
                    }
                    break;
            }
            AttackTimer--;
        }

        const int BOXING_RING_START_TIME = 180;
        const int BOXING_RING_ATTACK_COOLDOWN = 60; // 180 - 120
        void BoxingRing()
        {
            for (int i = 0; i < 64; i++)
            {
                Vector2 dustPos = arenaCenter + (Vector2.UnitY * BOXING_RING_ARENA_DISTANCE * 0.75f).RotatedBy(MathHelper.ToRadians(i * 360f / 64f));
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.GemDiamond);
                dust.noGravity = true;
            }
            switch (AttackTimer)
            {
                case BOXING_RING_START_TIME:
                    targetPosition = player.Center;
                    break;
                case > BOXING_RING_START_TIME - 60:
                    arenaCenter = targetPosition;
                    NPC.velocity = NPC.Center.DirectionTo(targetPosition + Vector2.UnitX * BOXING_RING_ARENA_DISTANCE) * NPC.Center.Distance(targetPosition + Vector2.UnitX * BOXING_RING_ARENA_DISTANCE) / 12;
                    break;
                case > BOXING_RING_START_TIME - 120:
                    NPC.velocity = Vector2.Zero;
                    break;
                case BOXING_RING_ATTACK_COOLDOWN:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        AttackCount = Main.rand.Next(-1, 2);
                    }
                    NPC.netUpdate = true;
                    break;
                case > BOXING_RING_ATTACK_COOLDOWN - 30:
                    targetPosition = player.Center + new Vector2(BOXING_RING_ARENA_DISTANCE, AttackCount * (BOXING_RING_ARENA_DISTANCE / 2));
                    NPC.velocity = NPC.Center.DirectionTo(targetPosition) * NPC.Center.Distance(targetPosition) / 12;
                    break;
                case > 0:
                    NPC.velocity = Vector2.Zero;
                    if (AttackTimer % 5 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            AttackCount2 = Main.rand.Next(-15, 15);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -Vector2.UnitX.RotatedBy(MathHelper.ToRadians(AttackCount2)), Proj["SpeedFlames"], NPC.damage / 2, 1, ai0: 0, ai1: 10, ai2: 0);
                        }
                        NPC.netUpdate = true;
                    }
                    break;
                case 0:
                    AttackTimer = BOXING_RING_ATTACK_COOLDOWN;
                    return;
            }

            AttackTimer--;
        }

        const int DASH2_COOLDOWN = 180;
        const int DASH2_START_TIME = 30;
        const int DASH2_SPEED = 50;
        const int DASH2_PAUSE_COUNT = 5;
        void DashAuraSpam()
        {
            switch (AttackTimer)
            {
                case > DASH2_START_TIME and < DASH2_COOLDOWN:
                    if (!spawnedAura)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["Aura"], NPC.damage, 1, ai1: NPC.whoAmI);
                        }
                        spawnedAura = true;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GemAmethyst);
                        dust.noGravity = true;
                        dust.velocity = new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));
                    }
                    break;
                case DASH2_START_TIME:
                    targetPosition = playerDirection;
                    break;
                case > 0 and < DASH2_START_TIME:
                    NPC.velocity = targetPosition.SafeNormalize(Vector2.Zero) * DASH2_SPEED;
                    break;
                case <= 0:
                    AttackCount++;
                    if ((AttackCount + 1) % DASH2_PAUSE_COUNT == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 offset = (Vector2.One * 800).RotatedBy(i * MathHelper.PiOver2 + (AttackCount + 1) * MathHelper.PiOver4);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center + offset, (player.Center + offset).DirectionTo(player.Center), ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 12);
                            }
                        }
                    }
                    if (AttackCount % DASH2_PAUSE_COUNT == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 offset = (Vector2.One * 800).RotatedBy(i * MathHelper.PiOver2 + AttackCount * MathHelper.PiOver4);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center + offset, (player.Center + offset).DirectionTo(player.Center) * (DASH2_SPEED / 2), Proj["Aura"], NPC.damage, 1, ai1: -1);
                            }
                        }
                        AttackTimer = DASH_COOLDOWN + 60;
                        NPC.velocity = Vector2.Zero;
                    }
                    else
                    {
                        AttackTimer = DASH_COOLDOWN;
                    }
                    return;
            }
            AttackTimer--;
        }

        const int LASER_SPAM_MOVE_START = 120;
        const int LASER_SPAM_ATTACK_START = 60;
        const int LASER_SPAM_ATTACK_COOLDOWN = 10;
        void LaserSpam()
        {
            switch (AttackTimer)
            {
                case LASER_SPAM_MOVE_START:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        targetPosition = NPC.Center + new Vector2(Main.rand.NextFloat(-300, 300), Main.rand.NextFloat(-300, 300));
                    }
                    NPC.netUpdate = true;
                    break;
                case > LASER_SPAM_ATTACK_START:
                    MoveToPos(targetPosition, 1.2f, 1.2f, 2, 2);
                    break;
                case LASER_SPAM_ATTACK_START:
                    targetPosition = playerDirection;
                    break;
                case > 0:
                    NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
                    if (AttackTimer % LASER_SPAM_ATTACK_COOLDOWN == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, targetPosition.SafeNormalize(Vector2.Zero), Proj["Beam"], NPC.damage, 1f, ai0: 30);
                        }
                        AttackCount++;
                        if (AttackCount < 6)
                        {
                            AttackTimer = LASER_SPAM_ATTACK_START;
                            return;
                        }
                        else
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.PiOver4), Proj["SpeedFlames"], NPC.damage, 1f, ai0: 30, ai1: 30, ai2: 1);
                                }
                            }
                            AttackTimer = LASER_SPAM_MOVE_START;
                            AttackCount = 0;
                            return;
                        }
                    }
                    break;
                case 0:
                    AttackTimer = LASER_SPAM_MOVE_START;
                    return;
            }
            AttackTimer--;
        }

        const int DASH_BLASTER_START_TIME = 240;
        const int DASH_BLASTER_MOVE_TIME = 180;
        const int DASH_BLASTER_DASH_SPEED = 20;
        void DashBlaster()
        {
            switch (AttackTimer)
            {
                case DASH_BLASTER_START_TIME:
                    if (!spawnedAura)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["Aura"], NPC.damage, 1, ai1: NPC.whoAmI);
                        }
                        spawnedAura = true;
                        targetPosition = NPC.Center - Vector2.UnitX * 300;
                    }
                    break;
                case > DASH_BLASTER_MOVE_TIME:
                    NPC.velocity = NPC.Center.DirectionTo(arenaCenter + Vector2.UnitX * DASH_BLASTER_ARENA_DISTANCE) * NPC.Center.Distance(player.Center + Vector2.UnitX * 700) / 12;
                    break;
                case > 0:
                    if (AttackTimer % 14 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item84 with { PitchRange = (-0.2f, 0.2f) });
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            targetPosition = new Vector2(NPC.Center.X, arenaCenter.Y) + new Vector2(-400, Main.rand.NextFloat(-DASH_BLASTER_ARENA_DISTANCE * 0.75f, DASH_BLASTER_ARENA_DISTANCE * 0.75f));
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.Pi / 4), Proj["SpeedFlames"], NPC.damage, 1f, ai0: 20, ai1: 35, ai2: 1);
                            }
                        }
                        NPC.netUpdate = true;
                    }
                    NPC.velocity = NPC.Center.DirectionTo(targetPosition) * DASH_BLASTER_DASH_SPEED;
                    break;
                case 0:
                    AttackTimer = DASH_BLASTER_START_TIME;
                    LemonUtils.DustCircle(NPC.Center, 16, 20, DustID.GemAmethyst, 1f, true);
                    LemonUtils.DustCircle(NPC.Center, 16, 15, DustID.GemDiamond, 1f, true);
                    LemonUtils.DustCircle(NPC.Center, 16, 10, DustID.GemAmethyst, 1f, true);
                    NPC.Center = arenaCenter + Vector2.UnitX * DASH_BLASTER_ARENA_DISTANCE;
                    NPC.netUpdate = true;
                    LemonUtils.DustCircle(NPC.Center, 16, 20, DustID.GemAmethyst, 1.5f, true);
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

        const float BASE_ARENA_DISTANCE = 3000;
        const float FB_COMBO_ARENA_DISTANCE = 1000;
        const float BOXING_RING_ARENA_DISTANCE = 300;
        const float LASER_SPAM_ARENA_DISTANCE = 1000;
        const float DASH_BLASTER_ARENA_DISTANCE = 1600;
        public void Arena()
        {
            float targetArenaDistance = BASE_ARENA_DISTANCE;
            arenaFollow = true;

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.FlameBeamCombo:
                        targetArenaDistance = FB_COMBO_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                }
            }
            else
            {
                switch (Attack)
                {
                    case (int)Attacks2.BoxingRing:
                        targetArenaDistance = BOXING_RING_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                    case (int)Attacks2.LaserSpam:
                        targetArenaDistance = LASER_SPAM_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                    case (int)Attacks2.DashBlaster:
                        targetArenaDistance = DASH_BLASTER_ARENA_DISTANCE;
                        arenaFollow = false;
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
                        var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 60f);

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

        void DialogueMessage(string key, int maxRand, int duration = 60)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                AdvancedPopupRequest message = new AdvancedPopupRequest();
                message.Color = Color.White;
                message.DurationInFrames = duration;
                message.Velocity = 10 * -Vector2.UnitY;
                int rand = Main.rand.Next(0, maxRand);
                string msgKey = key + rand;
                message.Text = Language.GetTextValue($"Mods.Paracosm.NPCs.NebulaMaster.Dialogue.{msgKey}");

                int index = PopupText.NewText(message, NPC.Center + -Vector2.UnitY * NPC.height / 2);
                Main.popupText[index].scale = 2f;
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
            int minFrame = 0;
            int maxFrame = 2;
            if (phase == 1)
            {
                if (AITimer > INTRO_DURATION && (Attack == (int)Attacks.SpeedingFlames || Attack == (int)Attacks.FlameBeamCombo) && AttackTimer < 30)
                {
                    minFrame = 3;
                    maxFrame = 5;
                }
                else if (AITimer > INTRO_DURATION && Attack == (int)Attacks.RapidDashes && AttackTimer < 30)
                {
                    minFrame = 6;
                    maxFrame = 8;
                }
                else
                {
                    minFrame = 0;
                    maxFrame = 2;
                }
            }
            else
            {
                if (AITimer > INTRO_DURATION && Attack == (int)Attacks2.BoxingRing && AttackTimer < 30)
                {
                    minFrame = 3;
                    maxFrame = 5;
                }
                else if (AITimer > INTRO_DURATION && Attack == (int)Attacks2.DashAuraSpam && AttackTimer < 30)
                {
                    minFrame = 6;
                    maxFrame = 8;
                }
                else
                {
                    minFrame = 0;
                    maxFrame = 2;
                }
            }
            int frameDur = 6;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > maxFrame * frameHeight)
                {
                    NPC.frame.Y = minFrame * frameHeight;
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
            DeleteProjectiles(Proj["Sphere"]);
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedNebulaMaster, -1);
            for (int i = 0; i < 16; i++)
            {
                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(0, NPC.width), Main.rand.Next(0, NPC.height)), new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(2f, 5f));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
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
        }
    }
}