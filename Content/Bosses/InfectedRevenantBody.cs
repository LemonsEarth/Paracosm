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

namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class InfectedRevenantBody : ModNPC
    {
        bool spawnedHeads = false;

        public Vector2 CorruptHeadPos = Vector2.Zero;
        public Vector2 CrimsonHeadPos = Vector2.Zero;
        public Player player;
        public Vector2 playerDirection;

        float AITimer = 0;

        List<Projectile> CursedFlames = new List<Projectile>();


        Vector2 ChosenPosition
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }

        InfectedRevenantCorruptHead corruptHead;
        InfectedRevenantCrimsonHead crimsonHead;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
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
            NPC.damage = (int)(NPC.damage * balance  * 0.5f);
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
            NPC.collideY = Collision.SolidCollision(new Vector2(NPC.position.X, NPC.position.Y + NPC.height), NPC.width, NPC.height / 4, true);


            if (NPC.Center.Distance(player.Center) > 2500)
            {
                NPC.EncourageDespawn(300);
            }

            if (player.dead)
            {
                NPC.active = false;
            }

            CorruptHeadPos = NPC.Center + new Vector2(-28, -20);
            CrimsonHeadPos = NPC.Center + new Vector2(28, -20);

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
            SpawnHeads();
            AITimer++;
        }

        public override bool? CanFallThroughPlatforms()
        {
            //return player.Center.Y > NPC.Center.Y + 35;
            return false;
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
            if (player == null)
            {
                return;
            }

            NPC corruptHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), CorruptHeadPos, CorruptHeadType());
            NPC crimsonHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), CrimsonHeadPos, CrimsonHeadType());

            corruptHead = (InfectedRevenantCorruptHead)corruptHeadNPC.ModNPC;
            crimsonHead = (InfectedRevenantCrimsonHead)crimsonHeadNPC.ModNPC;

            corruptHead.ParentIndex = NPC.whoAmI;
            crimsonHead.ParentIndex = NPC.whoAmI;
            corruptHead.NPC.damage = NPC.damage;
            crimsonHead.NPC.damage = NPC.damage;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: corruptHeadNPC.whoAmI, number2: crimsonHeadNPC.whoAmI);
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
            base.DrawBehind(index);
        }

        public override void FindFrame(int frameHeight)
        {
            /*int frameDur = 12;
            int startFrame = 0;
            int endFrame = 4;

            if (Movement == (float)MovementState.Flying)
            {
                startFrame = 5;
                endFrame = 10;
                frameDur = 6;

                if (NPC.frame.Y < startFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }

            if (NPC.frameCounter > frameDur)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > endFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }

            NPC.frameCounter++;*/
        }
    }
}
