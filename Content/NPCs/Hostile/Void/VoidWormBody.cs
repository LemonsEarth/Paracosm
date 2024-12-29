using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Void
{
    public class VoidWormBody : ModNPC
    {
        float AITimer = 0;

        int FollowingNPC
        {
            get { return (int)NPC.ai[1]; }
        }

        int FollowerNPC
        {
            get { return (int)NPC.ai[0]; }
        }

        int HeadNPC
        {
            get { return (int)NPC.ai[2]; }
        }

        int SegmentNum
        {
            get { return (int)NPC.ai[3]; }
        }

        int AttackTimer = 0;
        int AttackCount = 0;
        float RandNum = 0;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            Main.npcFrameCount[NPC.type] = 2;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 104;
            NPC.height = 52;
            NPC.Opacity = 1;
            NPC.lifeMax = 100000;
            NPC.defense = 80;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.DD2_BetsyDeath;
            NPC.value = 50000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 1;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void AI()
        {
            NPC followingNPC = Main.npc[FollowingNPC];

            if (followingNPC is null || !followingNPC.active || followingNPC.friendly || followingNPC.townNPC || followingNPC.lifeMax <= 5)
            {
                NPC.life = 0;
                NPC.active = false;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }

            FollowNextSegment(followingNPC);

            NPC.spriteDirection = followingNPC.spriteDirection;
            NPC.Opacity = (float)Math.Clamp(Math.Sin(MathHelper.ToRadians(AITimer * 3)), 0f, 1f);
            AITimer++;
        }

        public override bool CheckActive()
        {
            return false;
        }

        void FollowNextSegment(NPC followingNPC)
        {
            Vector2 toFollowing = followingNPC.Center - NPC.Center;
            NPC.rotation = toFollowing.ToRotation() + MathHelper.PiOver2;
            float distance = (toFollowing.Length() - (NPC.height - 20)) / toFollowing.Length();

            Vector2 pos = toFollowing * distance;
            NPC.velocity = Vector2.Zero;
            NPC.position += pos;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
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