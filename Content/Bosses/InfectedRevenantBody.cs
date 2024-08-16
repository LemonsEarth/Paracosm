using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Paracosm.Content.Projectiles.Hostile;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Terraria.DataStructures;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Projectiles;
using Paracosm.Common.Systems;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Microsoft.CodeAnalysis.Host.Mef;
using Paracosm.Content.Buffs;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.Graphics.CameraModifiers;
using System.IO;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Ranged;
using Paracosm.Content.Items.Weapons.Summon;

namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class InfectedRevenantBody : ModNPC
    {
        bool spawnedHeads = false;
        bool spawnedWings = false;

        public Vector2 CorruptHeadPos = Vector2.Zero;
        public Vector2 CrimsonHeadPos = Vector2.Zero;
        public Vector2 WingsPos = Vector2.Zero;
        public Player player;
        public Vector2 playerDirection;

        Vector2 crimsonHeadOffset = new Vector2(28, -20);
        Vector2 corruptHeadOffset = new Vector2(-28, -20);
        Vector2 wingsOffset = new Vector2(270, 152 + 72);

        ref float AITimer => ref NPC.ai[0];
        public ref float Attack => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float AttackDuration => ref NPC.ai[3];
        public int AttackCount = 0;

        public int phase = 1;

        bool phase2FirstTime = true;
        public bool phaseTransition = false;
        public float transitionDuration = 300;

        List<Projectile> CursedFlames = new List<Projectile>();
        Vector2 arenaCenter;

        int[] attackDurations = [600, 300, 600, 720, 720];

        public enum Attacks
        {
            SoaringBulletHell,
            DashingSpam,
            CorruptTorrent,
            SpiritWaves,
            FlameChase
        }

        InfectedRevenantCorruptHead corruptHead;
        InfectedRevenantCrimsonHead crimsonHead;
        InfectedRevenantWings wings;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "Paracosm/Content/Textures/Bestiary/InfectedRevenantBestiary",
                PortraitScale = 0.7f, // Portrait refers to the full picture when clicking on the icon in the bestiary
                PortraitPositionYOverride = 0f,
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("The infected remains of an ancient dragon, who cast aside his flesh to traverse a different realm."),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 236;
            NPC.height = 124;
            NPC.lifeMax = 140000;
            NPC.defense = 70;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 1000000;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 10;
            NPC.noGravity = true;
            NPC.SpawnWithHigherTime(2);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/EmbodimentOfEvil");
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AttackCount);
            writer.Write(phase);
            writer.Write(phase2FirstTime);
            writer.Write(phaseTransition);
            writer.Write(transitionDuration);
            writer.Write(elapsedTime);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AttackCount = reader.ReadInt32();
            phase = reader.ReadInt32();
            phase2FirstTime = reader.ReadBoolean();
            phaseTransition = reader.ReadBoolean();
            transitionDuration = reader.ReadSingle();
            elapsedTime = reader.ReadSingle();
        }

        public static int WingsType()
        {
            return ModContent.NPCType<InfectedRevenantWings>();
        }

        public static int CorruptHeadType()
        {
            return ModContent.NPCType<InfectedRevenantCorruptHead>();
        }

        public static int CrimsonHeadType()
        {
            return ModContent.NPCType<InfectedRevenantCrimsonHead>();
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 1f);
            NPC.damage = (int)(NPC.damage * balance * 0.5f);
            NPC.defense = 70;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];
            playerDirection = (player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero);


            if (NPC.life > (NPC.lifeMax * 0.66f))
            {
                phase = 1;
            }
            else if (phase2FirstTime)
            {
                phaseTransition = true;
                phase = 2;
                phase2FirstTime = false;
                NPC.netUpdate = true;
            }

            if (phaseTransition)
            {
                PhaseTransition();
            }

            if (NPC.Center.Distance(player.MountedCenter) > 5000)
            {
                NPC.active = false;
            }
            if (player.dead)
            {
                NPC.active = false;
            }

            if (phase == 1)
            {
                NPC.velocity.Y = 10;
                arenaCenter = NPC.Center;
            }

            if (phase == 2 && !phaseTransition)
            {
                if (AttackDuration <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SwitchAttacks();
                    }
                }
                switch (Attack)
                {
                    case (int)Attacks.SoaringBulletHell:
                        SoaringBulletHell();
                        break;
                    case (int)Attacks.DashingSpam:
                        DashingSpam();
                        break;
                    case (int)Attacks.CorruptTorrent:
                        CorruptTorrent();
                        break;
                    case (int)Attacks.SpiritWaves:
                        SpiritWaves();
                        break;
                    case (int)Attacks.FlameChase:
                        FlameChase();
                        break;
                }
                AttackDuration--;
            }

            SetOffsets();
            CursedFlamesArena();
            SpawnHeads();
            AITimer++;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return phase == 2 && !phaseTransition;
        }

        void SetOffsets()
        {
            CorruptHeadPos = NPC.Center + corruptHeadOffset;
            CrimsonHeadPos = NPC.Center + crimsonHeadOffset;
            WingsPos = NPC.Center - wingsOffset;
        }

        void SwitchAttacks()
        {
            Attack++;
            if (Attack > 4)
            {
                Attack = 0;
            }
            AttackDuration = attackDurations[(int)Attack];
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<DivineSpiritFlame>() || proj.type == ModContent.ProjectileType<CursedSpiritFlame>())
                {
                    proj.Kill();
                }
            }
            ResetVars();
        }

        void ResetVars()
        {
            AttackTimer = 0;
            NPC.alpha = 0;
            tempPlayerDir = Vector2.Zero;
            elapsedTime = 0;
            crimsonHead.ResetVars();
            corruptHead.ResetVars();
            NPC.netUpdate = true;
        }

        const int BulletHellCD = 10;
        Vector2 tempPlayerDir = Vector2.Zero;

        void SoaringBulletHell()
        {
            switch (AttackDuration)
            {
                case 600:
                    NPC.damage = 0;
                    break;
                case > 570:
                    NPC.velocity.Y = 3;
                    break;
                case 570:
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { MaxInstances = 1, Pitch = -0.1f }, NPC.Center);
                    break;
                case > 540:
                    NPC.velocity = ((player.MountedCenter - new Vector2(0, 800)) - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(player.MountedCenter - new Vector2(0, 800)) / 5);
                    if (NPC.alpha < 255)
                    {
                        NPC.alpha += 7;
                    }
                    break;
                case > 180:
                    NPC.dontTakeDamage = true;
                    NPC.velocity = ((arenaCenter - new Vector2(0, 1400)) - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(arenaCenter - new Vector2(0, 1400)) / 5);
                    if (AttackTimer == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i < 2; i += 2)
                            {
                                Vector2 spawnPos = arenaCenter + new Vector2(0, i * 1140).RotatedBy(MathHelper.ToRadians(10 * AttackCount));
                                var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spawnPos, (player.MountedCenter - spawnPos).SafeNormalize(Vector2.Zero) * 2, ModContent.ProjectileType<CursedSpiritFlame>(), corruptHead.NPC.damage, 1, -1, 30, player.MountedCenter.X - spawnPos.X, player.MountedCenter.Y - spawnPos.Y);
                                CursedSpiritFlame cfr = (CursedSpiritFlame)proj.ModProjectile;
                                cfr.speed = 40;
                            }

                            if (AttackCount % 5 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), player.MountedCenter + new Vector2(0, 400).RotatedBy(AttackCount * MathHelper.PiOver2), Vector2.Zero, ModContent.ProjectileType<DivineSpiritFlame>(), crimsonHead.NPC.damage, 1, ai0: 30, ai1: 3, ai2: player.whoAmI);
                            }
                        }
                        AttackCount++;
                        AttackTimer = BulletHellCD;
                    }
                    AttackTimer--;
                    break;
                case 180:
                    SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 2, Pitch = -1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.NPCDeath62 with { MaxInstances = 2, Pitch = -0.5f }, NPC.Center);
                    NPC.dontTakeDamage = false;
                    NPC.velocity = ((arenaCenter - new Vector2(0, 1400)) - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(arenaCenter - new Vector2(0, 1400)) / 5);
                    break;
                case > 170:
                    NPC.alpha = 0;
                    break;
                case 170:
                    NPC.netUpdate = true;
                    break;
                case > 110:
                    NPC.velocity = (arenaCenter - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(arenaCenter) / 20);
                    Lighting.AddLight(NPC.Center, 10, 10, 10);
                    var green = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.CursedTorch, Scale: 4f);
                    green.noGravity = true;
                    var orange = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.OrangeTorch, Scale: 4f);
                    orange.noGravity = true;
                    break;
                case 110:
                    AttackCount = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            for (float i = -1; i <= 1; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * AttackCount * 0.2f, -1) * 10, ProjectileID.GoldenShowerHostile, (int)(crimsonHead.NPC.damage * 0.8f), 1);
                            }
                            AttackCount++;
                        }

                    }
                    break;
                case > 0:
                    NPC.velocity = (arenaCenter - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(arenaCenter) / 12);
                    break;
            }

        }

        const int DashingCD = 90;

        void DashingSpam()
        {
            NPC.damage = 0;
            switch (AttackTimer)
            {
                case > 0:
                    NPC.velocity /= 1.1f;
                    break;
                case <= 0:
                    tempPlayerDir = playerDirection;
                    NPC.velocity = tempPlayerDir.SafeNormalize(Vector2.Zero) * 40;
                    AttackTimer = DashingCD;
                    SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 2, Pitch = -0.3f }, NPC.Center);
                    break;
            }
            AttackTimer--;
        }

        void CorruptTorrent()
        {
            NPC.velocity = (arenaCenter - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(arenaCenter) / 6);
        }

        void SpiritWaves()
        {
            Vector2 leftPos = arenaCenter - new Vector2(600, 0);
            Vector2 rightPos = arenaCenter + new Vector2(600, 0);
            switch (AttackDuration)
            {
                case > 480:
                    NPC.velocity = (leftPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(leftPos) / 10;
                    break;
                case > 240:
                    NPC.velocity = (arenaCenter - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(arenaCenter) / 10;
                    break;
                case > 0:
                    NPC.velocity = (rightPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(rightPos) / 10;
                    break;
            }
        }
        float elapsedTime = 0;
        Vector2 leftPos = Vector2.Zero;
        Vector2 rightPos = Vector2.Zero;

        void FlameChase()
        {
            Vector2 topPos = arenaCenter + new Vector2(0, -1000);
            Vector2 botPos = arenaCenter + new Vector2(0, 1000);
            float percentComplete = 0;
            switch (AttackDuration)
            {
                case > 660:
                    leftPos = new Vector2(arenaCenter.X - 1200, player.MountedCenter.Y - 400);
                    rightPos = new Vector2(arenaCenter.X + 1200, player.MountedCenter.Y - 400);
                    NPC.velocity = (leftPos - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(leftPos) / 20);
                    break;
                case > 540:
                    elapsedTime++;
                    percentComplete = elapsedTime / 120;
                    NPC.Center = Vector2.SmoothStep(leftPos, rightPos, MathHelper.SmoothStep(0, 1, percentComplete));
                    break;
                case > 480:
                    elapsedTime = 0;
                    leftPos = new Vector2(arenaCenter.X, player.MountedCenter.Y - 300);
                    rightPos = new Vector2(arenaCenter.X + 1200, player.MountedCenter.Y - 300);
                    NPC.velocity = (rightPos - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(rightPos) / 20);
                    break;
                case > 360:
                    elapsedTime++;
                    percentComplete = elapsedTime / 60;
                    NPC.Center = Vector2.SmoothStep(rightPos, leftPos, MathHelper.SmoothStep(0, 1, percentComplete));
                    break;
                case > 300:
                    elapsedTime = 0;
                    NPC.velocity = (topPos - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(topPos) / 20);
                    break;
                case > 180:
                    elapsedTime++;
                    percentComplete = elapsedTime / 120;
                    NPC.Center = Vector2.SmoothStep(topPos, arenaCenter, MathHelper.SmoothStep(0, 1, percentComplete));
                    break;
                case > 120:
                    elapsedTime = 0;
                    NPC.velocity = (botPos - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(botPos) / 20);
                    break;
                case > 0:
                    elapsedTime++;
                    percentComplete = elapsedTime / 120;
                    NPC.Center = Vector2.SmoothStep(botPos, arenaCenter, MathHelper.SmoothStep(0, 1, percentComplete));
                    break;
            }
        }

        float tempVolume;
        void PhaseTransition()
        {
            NPC.dontTakeDamage = true;
            switch (transitionDuration)
            {
                case 300:
                    NPC.netUpdate = true;
                    tempVolume = Main.musicVolume;
                    Attack = -1;
                    break;
                case > 150:
                    if (Main.musicVolume > 0)
                    {
                        Main.musicVolume = MathHelper.Lerp(Main.musicVolume, 0, 0.005f);
                    }
                    break;
                case 150:
                    SpawnWings();
                    SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 2, Pitch = -1f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.NPCDeath62 with { MaxInstances = 2, Pitch = -0.5f }, NPC.Center);
                    if (NPC.life < NPC.lifeMax * 0.65f)
                    {
                        NPC.life = (int)Math.Ceiling((double)NPC.lifeMax * 0.65f);
                    }
                    break;
                case > 20:
                    PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 15f, 6f, 20, 1000f, FullName);
                    Main.instance.CameraModifiers.Add(modifier);
                    for (int i = 0; i < 5; i++)
                    {
                        var green = Dust.NewDustDirect(NPC.position + new Vector2(95, 0), 2, 2, DustID.CursedTorch, Scale: 4f);
                        green.velocity = new Vector2(0, -Main.rand.Next(20, 30)).RotatedByRandom(2 * MathHelper.Pi);
                        var orange = Dust.NewDustDirect(NPC.position + new Vector2(95, 0), 2, 2, DustID.OrangeTorch, Scale: 4f);
                        orange.velocity = new Vector2(0, -Main.rand.Next(20, 30)).RotatedByRandom(2 * MathHelper.Pi);
                    }
                    break;
                case > 0:
                    Main.musicVolume = tempVolume;
                    break;
                case <= 0:
                    phaseTransition = false;
                    NPC.dontTakeDamage = false;
                    NPC.netUpdate = true;
                    break;
            }

            transitionDuration--;
        }

        void CursedFlamesArena()
        {
            if (CursedFlames.Count < 20)
            {
                for (int i = 0; i < 20; i++)
                {
                    var cursedFlame = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -1180).RotatedBy(i * MathHelper.ToRadians(18)), Vector2.Zero, ModContent.ProjectileType<CursedFlamesBorder>(), 100, 1);
                    CursedFlames.Add(cursedFlame);
                }
            }

            for (int i = 0; i < CursedFlames.Count; i++)
            {
                Vector2 pos = arenaCenter + new Vector2(0, -1180).RotatedBy(i * MathHelper.ToRadians(18)).RotatedBy(MathHelper.ToRadians(AITimer));
                CursedFlames[i].velocity = (pos - CursedFlames[i].position).SafeNormalize(Vector2.Zero) * (CursedFlames[i].position.Distance(pos) / 20);

                CursedFlames[i].timeLeft = 180;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (arenaCenter.Distance(player.MountedCenter) > 1200)
                {
                    player.AddBuff(ModContent.BuffType<Infected>(), 2);
                }
            }
        }

        void SpawnHeads()
        {
            if (spawnedHeads)
            {
                return;
            }

            spawnedHeads = true;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            NPC corruptHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), CorruptHeadPos, CorruptHeadType(), NPC.whoAmI, NPC.whoAmI);
            corruptHead = (InfectedRevenantCorruptHead)corruptHeadNPC.ModNPC;
            corruptHead.ParentIndex = NPC.whoAmI;
            corruptHead.NPC.damage = NPC.damage;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: corruptHeadNPC.whoAmI);
            }

            NPC crimsonHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), CrimsonHeadPos, CrimsonHeadType(), NPC.whoAmI, NPC.whoAmI);
            crimsonHead = (InfectedRevenantCrimsonHead)crimsonHeadNPC.ModNPC;
            crimsonHead.ParentIndex = NPC.whoAmI;
            crimsonHead.NPC.damage = NPC.damage;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: crimsonHeadNPC.whoAmI);
            }
        }

        void SpawnWings()
        {
            if (spawnedWings)
            {
                return;
            }

            spawnedWings = true;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            NPC wingsNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), WingsPos, WingsType(), NPC.whoAmI, NPC.whoAmI);
            wings = (InfectedRevenantWings)wingsNPC.ModNPC;
            wings.ParentIndex = NPC.whoAmI;
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: wingsNPC.whoAmI);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    var green = Dust.NewDustDirect(NPC.position + new Vector2(95, 0), 2, 2, DustID.CursedTorch, Scale: 4f);
                    green.velocity = new Vector2(0, -Main.rand.Next(20, 30)).RotatedByRandom(2 * MathHelper.Pi);
                    var orange = Dust.NewDustDirect(NPC.position + new Vector2(95, 0), 2, 2, DustID.OrangeTorch, Scale: 4f);
                    orange.velocity = new Vector2(0, -Main.rand.Next(20, 30)).RotatedByRandom(2 * MathHelper.Pi);
                }
                SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 2, Pitch = -1f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath62 with { MaxInstances = 2, Pitch = -0.5f }, NPC.Center);

                int goreType1 = Mod.Find<ModGore>("InfectedRevenantBody_Gore1").Type;
                int goreType2 = Mod.Find<ModGore>("InfectedRevenantBody_Gore2").Type;
                int corruptHeadGoreType = Mod.Find<ModGore>("InfectedRevenantCorruptHead_Gore").Type;
                int crimsonHeadGoreType = Mod.Find<ModGore>("InfectedRevenantCrimsonHead_Gore").Type;

                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), goreType1);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), goreType2);
                }
                Gore.NewGore(NPC.GetSource_Death(), corruptHead.NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), corruptHeadGoreType);
                Gore.NewGore(NPC.GetSource_Death(), crimsonHead.NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), crimsonHeadGoreType);
            }
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                corruptHead.NPC.StrikeInstantKill();
                crimsonHead.NPC.StrikeInstantKill();
            }
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedInfectedRevenant, -1);
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (phase - 1) * frameHeight;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NightmareScale>(), 1, 15, 25));
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DivineFlesh>(), 1, 15, 25));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<InfectedRevenantBossBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.InfectedRevenantRelic>()));
        }
    }
}
