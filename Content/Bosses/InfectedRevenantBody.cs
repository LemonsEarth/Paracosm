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
using Paracosm.Content.Items.Weapons;
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
        ref float P2Timer => ref NPC.ai[1];

        public int phase = 1;

        bool phase2FirstTime = true;
        public bool phaseTransition = false;
        public float transitionDuration = 300;

        List<Projectile> CursedFlames = new List<Projectile>();

        InfectedRevenantCorruptHead corruptHead;
        InfectedRevenantCrimsonHead crimsonHead;
        InfectedRevenantWings wings;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 236;
            NPC.height = 124;
            NPC.lifeMax = 100000;
            NPC.defense = 30;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 1000000;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(2);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/EmbodimentOfEvil");
            }
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
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.7f);
            NPC.damage = (int)(NPC.damage * balance * 0.5f);
            NPC.defense = 50;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];
            playerDirection = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);


            if (NPC.life > (NPC.lifeMax / 2))
            {
                phase = 1;
            }
            else if (phase2FirstTime)
            {
                phaseTransition = true;
                phase = 2;
                phase2FirstTime = false;
            }

            if (phaseTransition)
            {
                PhaseTransition();
            }

            if (NPC.Center.Distance(player.Center) > 2500)
            {
                NPC.EncourageDespawn(300);
            }
            if (player.dead)
            {
                NPC.active = false;
            }

            if (phase == 1)
            {
                NPC.velocity.Y = 10;
            }

            if (phase == 2 && !phaseTransition)
            {
                NPC.velocity = NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 5;
                P2Timer++;
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


        float tempVolume;
        void PhaseTransition()
        {
            NPC.dontTakeDamage = true;
            switch (transitionDuration)
            {
                case 300:
                    tempVolume = Main.musicVolume;
                    break;
                case > 150:
                    if (Main.musicVolume > 0)
                    {
                        Main.musicVolume = MathHelper.Lerp(Main.musicVolume, 0, 0.005f);
                    }
                    break;
                case 150:
                    SpawnWings();
                    SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 2, Pitch = -0f });
                    SoundEngine.PlaySound(SoundID.NPCDeath62 with { MaxInstances = 2, Pitch = -0.5f });
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
                    Main.musicVolume = 1;
                    break;
                case <= 0:
                    phaseTransition = false;
                    NPC.dontTakeDamage = false;
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
                CursedFlames[i].position = NPC.Center + new Vector2(0, -1180).RotatedBy(i * MathHelper.ToRadians(18)).RotatedBy(MathHelper.ToRadians(AITimer));
                CursedFlames[i].timeLeft = 180;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.Center) > 1200)
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

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                corruptHead.NPC.StrikeInstantKill();
                crimsonHead.NPC.StrikeInstantKill();
            }
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (phase - 1) * frameHeight;
        }
    }
}
