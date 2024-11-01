using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Biomes;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Mono.Cecil;
using System;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using System.Collections.Generic;

namespace Paracosm.Content.NPCs.Hostile
{
    public class Wanderer : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        ref float speed => ref NPC.ai[1];
        ref float randomChoice => ref NPC.ai[2];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.width = 28;
            NPC.height = 50;
            NPC.lifeMax = 160;
            NPC.defense = 4;
            NPC.damage = 30;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.value = 200;
            NPC.aiStyle = -1;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<ParacosmicDistortion>().Type };
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0.7f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ParacosmicDistortion>().ModBiomeBestiaryInfoElement),
                });
        }

        public override void OnSpawn(IEntitySource source)
        {
            speed = 1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Parashard>(), minimumDropped: 1, maximumDropped: 6));
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255)
            {
                NPC.TargetClosest(false);
            }
            Player player = Main.player[NPC.target];

            NPC.spriteDirection = Math.Sign(NPC.Center.DirectionTo(player.MountedCenter).X);
            if (AITimer % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                randomChoice = Main.rand.Next(0, 2);
                NPC.netUpdate = true;
            }
            AITimer++;

            if (randomChoice == 1 && NPC.HasPlayerTarget)
            {
                Chase(player);
            }
            else
            {
                Idle();
            }


            Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height), NPC.width, 0, DustID.Corruption);
        }

        void Idle()
        {
            NPC.velocity = new Vector2(0, (float)Math.Sin(MathHelper.ToRadians(AITimer)));
            NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, MathHelper.ToRadians(5));
            speed = 1;
        }

        void Chase(Player player)
        {
            if (AITimer == 0 || AITimer % 60 == 0)
            {
                speed = 1;
            }
            NPC.velocity = NPC.Center.DirectionTo(player.MountedCenter) * speed;
            if (speed > 3)
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.spriteDirection * MathHelper.ToRadians(30), MathHelper.ToRadians(1));
            }
            else
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, MathHelper.ToRadians(1));
            }
            speed += 0.2f;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 30;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 1 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
    }
}
