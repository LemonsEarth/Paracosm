﻿using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Bosses.StardustLeviathan
{
    public class StardustLeviathanTail : ModNPC
    {
        float AITimer = 0;

        int FollowingNPC
        {
            get { return (int)NPC.ai[1]; }
        }

        int HeadNPC
        {
            get { return (int)NPC.ai[2]; }
        }

        StardustLeviathanHead head;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 128;
            NPC.height = 64;
            NPC.Opacity = 1;
            NPC.lifeMax = 1500000;
            NPC.defense = 60;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit56;
            NPC.DeathSound = SoundID.NPCDeath60;
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
            writer.Write(NPC.Opacity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.Opacity = reader.ReadSingle();
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void AI()
        {
            NPC followingNPC = Main.npc[FollowingNPC];
            NPC headNPC = Main.npc[HeadNPC];

            if (followingNPC is null || !followingNPC.active || followingNPC.friendly || followingNPC.townNPC || followingNPC.lifeMax <= 5)
            {
                NPC.life = 0;
                NPC.active = false;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
            head = (StardustLeviathanHead)headNPC.ModNPC;

            NPC.Opacity = head.NPC.Opacity;
            if (AITimer % 10 == 0)
            {
                NPC.netUpdate = true;
            }
            FollowNextSegment(followingNPC);

            NPC.spriteDirection = followingNPC.spriteDirection;

            AITimer++;
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

        public override bool CheckActive()
        {
            return false;
        }

        public override bool CheckDead()
        {
            Filters.Scene.Deactivate("Paracosm:ScreenTintShader");
            return true;
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