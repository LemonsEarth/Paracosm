using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.ComponentModel;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Bosses.VortexMothership
{
    public class VortexUFO : ModNPC
    {
        public int ParentIndex
        {
            get => (int)NPC.ai[0];
            set
            {
                NPC.ai[0] = value;
            }
        }
        ref float AttackTimer => ref NPC.ai[1];
        ref float AttackTimer2 => ref NPC.ai[2];
        ref float RandNum => ref NPC.ai[3];
        float AITimer = 0;
        Vector2 shootDirection = Vector2.Zero;
        Vector2 playerDirection => NPC.Center.DirectionTo(body.player.Center);
        VortexMothershipBody body;

        Vector2[] offSets { get; } =
        {
            new Vector2(0, -500),
            new Vector2(0, 500),
            new Vector2(-500, 0),
            new Vector2(500, 0),
            new Vector2(-500, -500),
            new Vector2(-500, 500),
            new Vector2(500, -500),
            new Vector2(500, 500),
        };

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
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
            NPC.width = 60;
            NPC.height = 60;
            NPC.lifeMax = 15000;
            NPC.defense = 50;
            NPC.value = 0;
            NPC.damage = 0;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 1;
            NPC.hide = true;
            NPC.netAlways = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
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
            if (shootDirection == Vector2.Zero) shootDirection = playerDirection; // By default, face the player
            NPC.rotation = shootDirection.ToRotation() - MathHelper.PiOver2;

            if (AITimer % 300 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    RandNum = Main.rand.Next(0, 8);
                }
                AttackTimer = 0;
                AttackTimer2 = 0;
            }

            if (AttackTimer < 100)
            {
                MoveToPos(body.player.Center + offSets[(int)RandNum]);
                shootDirection = playerDirection;
            }
            else
            {
                NPC.velocity = Vector2.Zero;
                if (AttackTimer2 <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 30, ModContent.ProjectileType<TeslaShot>(), body.damage, 1);
                    }
                    AttackTimer2 = 20;
                }
                AttackTimer2--;
            }
            AttackTimer++;
            AITimer++;
        }

        void MoveToPos(Vector2 pos)
        {
            Vector2 direction = NPC.Center.DirectionTo(pos);
            float XaccelMod = Math.Sign(direction.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(direction.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2(XaccelMod * 2f + 1f * Math.Sign(direction.X), YaccelMod * 2f + 1f * Math.Sign(direction.Y));
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
