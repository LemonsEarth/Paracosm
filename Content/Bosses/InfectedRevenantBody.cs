﻿using System;
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

namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class InfectedRevenantBody : ModNPC
    {
        bool spawnedHeads = false;

        public Vector2 HeadPos = Vector2.Zero;
        public Player player;
        public Vector2 playerDirection;

        float AITimer = 0;
        float attackTimer = 0;
        public ref float Movement => ref NPC.ai[1];
        Vector2 ChosenPosition
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }

        enum MovementState
        {
            Grounded,
            Flying
        }

        InfectedRevenantCorruptHead corruptHead;
        InfectedRevenantCrimsonHead crimsonHead;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 11;
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


        float speed = 1;
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

            if (AITimer % 600 == 0)
            {
                attackTimer = 0;
                if (Movement == 0)
                {
                    Movement = 1;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        ChosenPosition = RandomPosition();
                        NPC.netUpdate = true;
                    }
                    NPC.velocity.Y = 0;
                    speed = 1;
                }
                else Movement = 0;
            }

            switch (Movement)
            {
                case (float)MovementState.Grounded:
                    NPC.noTileCollide = false;
                    NPC.velocity = new Vector2(playerDirection.X, 1) * 2;
                    attackTimer++;
                    break;
                case (float)MovementState.Flying:
                    NPC.noTileCollide = false;
                    NPC.velocity = playerDirection * speed;

                    if (attackTimer > 60)
                    {
                        speed = 16;
                    }
                    else
                    {
                        speed = 3;
                    }
                    if (attackTimer == 90)
                    {
                        attackTimer = 0;
                    }
                    attackTimer++;
                    if (attackTimer % 10 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center - new Vector2(0, 1000), new Vector2(0, 2), ProjectileID.GoldenShowerHostile, 50, 10);
                            NPC.netUpdate = true;
                        }
                    }
                    break;
            }

            HeadPos = NPC.Center - new Vector2(-Math.Sign(playerDirection.X) * 90, 10);
            SpawnHeads();
            AITimer++;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return Movement == (float)MovementState.Flying;
        }

        Vector2 RandomPosition()
        {
            Vector2 randomPos = HeadPos + new Vector2(Main.rand.Next(-300, 300), Main.rand.Next(-300, 300));

            return randomPos;
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

            NPC.frameCounter++;
        }
    }
}