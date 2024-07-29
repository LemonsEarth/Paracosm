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

namespace Paracosm.Content.Bosses
{
    public class InfectedRevenantCrimsonHead : ModNPC
    {
        public int ParentIndex;

        ref float AITimer => ref NPC.ai[0];
        ref float Attack => ref NPC.ai[0];
        Vector2 ChosenPosition
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 72;
            NPC.height = 72;
            NPC.lifeMax = 100000;
            NPC.dontTakeDamage = true;
            NPC.defense = 30;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 1000000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(2);

            NPC.hide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public static int BodyType()
        {
            return ModContent.NPCType<InfectedRevenantBody>();
        }

        public override void AI()
        {
            NPC bodyNPC = Main.npc[ParentIndex];
            InfectedRevenantBody body = (InfectedRevenantBody)bodyNPC.ModNPC;
            if (AITimer % 40 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    ChosenPosition = RandomPosition(body);
                    NPC.netUpdate = true;
                }
            }
            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 10;
            NPC.velocity += body.NPC.velocity;
            NPC.rotation = NPC.Center.DirectionTo(body.player.Center).ToRotation() + MathHelper.PiOver2;
            if ((body.player.Center - NPC.Center).X <= 0)
            {
                NPC.spriteDirection = -1;
            }
            else
            {
                NPC.spriteDirection = 1;
            }

            AITimer++;
        }

        Vector2 RandomPosition(InfectedRevenantBody body)
        {
            if (body == null)
            {
                return NPC.Center;
            }
            Vector2 randomPos = body.HeadPos + new Vector2(-Math.Sign(body.playerDirection.X) * Main.rand.Next(-100, 50), Main.rand.Next(-50, 25));

            return randomPos;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
