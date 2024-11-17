using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Bosses.VortexMothership;
using Paracosm.Content.Projectiles.Hostile;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Bosses.VortexMothership
{
    public class VortexTeslaGun : ModNPC
    {
        public int ParentIndex
        {
            get => (int)NPC.ai[0];
            set
            {
                NPC.ai[0] = value;
            }
        }
        float AITimer = 0;
        public ref float Attack => ref NPC.ai[1];
        public ref float AttackCount => ref NPC.ai[2];
        public float attackDuration = 0;
        public ref float AttackTimer => ref NPC.ai[3];
        int[] attackDurations = { 300, 300, 360, 400, 600 };

        VortexMothershipBody body;

        enum Attacks
        {

        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawMods = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawMods);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 100;
            NPC.height = 100;
            NPC.lifeMax = 100000;
            NPC.dontTakeDamage = true;
            NPC.defense = 30;
            NPC.value = 0;
            NPC.damage = 0;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1;
            NPC.noGravity = true;
            NPC.npcSlots = 1;
            NPC.SpawnWithHigherTime(2);
            NPC.hide = true;
            NPC.netAlways = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AITimer);
            writer.Write(AttackTimer);
            writer.Write(attackDuration);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AITimer = reader.ReadSingle();
            AttackTimer = reader.ReadSingle();
            attackDuration = reader.ReadSingle();
        }

        public static int BodyType()
        {
            return ModContent.NPCType<VortexMothershipBody>();
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void AI()
        {
            NPC bodyNPC = Main.npc[ParentIndex];
            if (Main.netMode != NetmodeID.MultiplayerClient && (Main.npc[ParentIndex] == null || !Main.npc[ParentIndex].active || Main.npc[ParentIndex].type != BodyType()))
            {
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                return;
            }
            body = (VortexMothershipBody)bodyNPC.ModNPC;
            NPC.Opacity = body.NPC.Opacity;
            NPC.rotation = NPC.Center.DirectionTo(body.player.Center).ToRotation() + MathHelper.PiOver2;
            NPC.Center = body.teslaGunPos;
            if (AITimer < 60)
            {
                AITimer++;
                return;
            }

            if (AITimer % 60 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.Center.DirectionTo(body.player.Center) * 10, ModContent.ProjectileType<TeslaShot>(), body.damage, 1);
                }
            }

            attackDuration--;
            AITimer++;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 6;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            return true;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
