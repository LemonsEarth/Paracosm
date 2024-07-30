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
using Newtonsoft.Json.Linq;

namespace Paracosm.Content.Bosses
{
    public class InfectedRevenantCorruptHead : ModNPC
    {
        private const string NeckTexturePath = "Paracosm/Content/Bosses/InfectedRevenantCorruptNeck";
        private static Asset<Texture2D> NeckTexture;

        public int ParentIndex;
        ref float AITimer => ref NPC.ai[0];
        ref float Attack => ref NPC.ai[1];
        public InfectedRevenantBody body;
        Vector2 ChosenPosition
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }

        public override void Load()
        {
            NeckTexture = ModContent.Request<Texture2D>(NeckTexturePath);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawMods = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawMods);
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
            this.body = body;
            if (AITimer % 30 == 0)
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

            if (AITimer % 30 == 0)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10, ProjectileID.CursedFlameHostile, 50, 10);
            }

            AITimer++;
        }

        Vector2 RandomPosition(InfectedRevenantBody body)
        {
            if (body == null)
            {
                return NPC.Center;
            }
            Vector2 randomPos = body.HeadPos + new Vector2(-Math.Sign(body.playerDirection.X) * Main.rand.Next(-100, -80), Main.rand.Next(-50, 10));

            return randomPos;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (body == null)
            {
                return true;
            }

            Vector2 drawPosition = NPC.Center;
            Vector2 HeadToNeckBase = body.HeadPos - NPC.Center;
            float rotation = HeadToNeckBase.SafeNormalize(Vector2.Zero).ToRotation() + MathHelper.PiOver2;
            float segmentHeight = NeckTexture.Value.Height;
            float drawnSegments = 0;
            float distanceLeft = HeadToNeckBase.Length() + segmentHeight / 2;
            if (segmentHeight == 0)
            {
                segmentHeight = 24;
            }

            while (distanceLeft > 0f)
            {
                drawPosition += HeadToNeckBase.SafeNormalize(Vector2.Zero) * segmentHeight;
                distanceLeft = drawPosition.Distance(body.HeadPos);
                drawnSegments++;
                distanceLeft -= segmentHeight;
                spriteBatch.Draw(NeckTexture.Value, drawPosition - screenPos, null, new Color(255 - drawnSegments * 10, 255 - drawnSegments * 10, 255 - drawnSegments * 10), rotation, new Vector2(NeckTexture.Value.Width / 2f, NeckTexture.Value.Height / 2f), 1f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
