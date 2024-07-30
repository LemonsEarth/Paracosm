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

namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class InfectedRevenantBody : ModNPC
    {
        bool spawnedHeads = false;

        public Vector2 HeadPos = Vector2.Zero;
        public Player player;
        public Vector2 playerDirection;

        ref float AITimer => ref NPC.ai[0];

        InfectedRevenantCorruptHead corruptHead;
        InfectedRevenantCrimsonHead crimsonHead;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
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
            NPC.width = 243;
            NPC.height = 160;
            NPC.lifeMax = 100000;
            NPC.defense = 30;
            NPC.damage = 80;
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


        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            player = Main.player[NPC.target];

            playerDirection = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            if (playerDirection.X <= 0)
            {
                NPC.spriteDirection = -1;
            }
            else
            {
                NPC.spriteDirection = 1;
            }

            HeadPos = NPC.Center - new Vector2(-Math.Sign(playerDirection.X) * 90, 10);
            SpawnHeads();
            AITimer++;
        }

        void SpawnHeads()
        {
            if (player == null)
            {
                return;
            }

            if (spawnedHeads)
            {
                return;
            }

            spawnedHeads = true;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            NPC corruptHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, CorruptHeadType());
            NPC crimsonHeadNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, CrimsonHeadType());

            corruptHead = (InfectedRevenantCorruptHead)corruptHeadNPC.ModNPC;
            crimsonHead = (InfectedRevenantCrimsonHead)crimsonHeadNPC.ModNPC;

            corruptHead.ParentIndex = NPC.whoAmI;
            crimsonHead.ParentIndex = NPC.whoAmI;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: corruptHeadNPC.whoAmI);
                NetMessage.SendData(MessageID.SyncNPC, number: crimsonHeadNPC.whoAmI);
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
            int frameDur = 12;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 4 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
    }
}
