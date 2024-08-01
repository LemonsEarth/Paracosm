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
        Vector2 defaultHeadPos = Vector2.Zero;

        float attackDuration = 0;
        float attackTimer = 0;
        int[] attackDurations = { 300, 300, 300, 300, 300 };

        const int cursedBurstCD1 = 20;
        const int cursedBurstCD2 = 60;
        int cursedBurstCount = 0;

        enum Attacks
        {
            CursedBurstFire,
            SpinCursedCross,
            Attack3,
            Attack4,
            Attack5,
        }

        Queue<int> AttackOrder = new Queue<int>();

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
            NPC.width = 58;
            NPC.height = 52;
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
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                while (AttackOrder.Count < 5)
                {
                    int attackNum = Main.rand.Next(0, 5);
                    if (!AttackOrder.Contains(attackNum))
                    {
                        AttackOrder.Enqueue(attackNum);
                    }
                }
                NPC.netUpdate = true;
            }
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
            defaultHeadPos = body.CorruptHeadPos - new Vector2(0, 120);
            if (attackDuration == 0)
            {
                attackTimer = 0;
                Attack = AttackOrder.Dequeue();
                AttackOrder.Enqueue((int)Attack);
                attackDuration = attackDurations[(int)Attack];
                SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = -0.2f });
            }

            switch (Attack)
            {
                case (float)Attacks.CursedBurstFire:
                    CursedBurstFire();
                    break;
                case (float)Attacks.SpinCursedCross:
                    SpinCursedCross();
                    break;
                case (float)Attacks.Attack3:
                    CursedBurstFire();
                    break;
                case (float)Attacks.Attack4:
                    SpinCursedCross();
                    break;
                case (float)Attacks.Attack5:
                    CursedBurstFire();
                    break;
            }
            attackDuration--;
            AITimer++;
        }

        Vector2 RandomPosition(InfectedRevenantBody body)
        {
            if (body == null)
            {
                return NPC.Center;
            }
            Vector2 randomPos = body.CorruptHeadPos + new Vector2(Main.rand.Next(-150, 30), Main.rand.Next(-300, 30));

            return randomPos;
        }

        void CursedBurstFire()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (attackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10, ProjectileID.CursedFlameHostile, 50, 10);
                    cursedBurstCount++;
                    attackTimer = cursedBurstCD1;
                    if (cursedBurstCount >= 4)
                    {
                        cursedBurstCount = 0;
                        attackTimer = cursedBurstCD2;
                    }
                    NPC.netUpdate = true;
                }
            }
            attackTimer--;
        }

        void SpinCursedCross()
        {
            Vector2 position = defaultHeadPos + new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(attackTimer));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12; ;
            if (attackTimer % 45 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver2) * 20, ProjectileID.CursedFlameHostile, 50, 10);
                    }
                    NPC.netUpdate = true;
                }
            }
            attackTimer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (body == null)
            {
                return true;
            }

            Vector2 drawPosition = body.CorruptHeadPos;
            Vector2 NeckBaseToHead = NPC.Center - body.CorruptHeadPos;
            float rotation = NeckBaseToHead.SafeNormalize(Vector2.Zero).ToRotation() + MathHelper.PiOver2;
            float segmentHeight = NeckTexture.Value.Height;
            float drawnSegments = 0;
            float distanceLeft = NeckBaseToHead.Length() + segmentHeight / 2;
            if (segmentHeight == 0)
            {
                segmentHeight = 24;
            }

            while (distanceLeft > 0f)
            {
                drawPosition += NeckBaseToHead.SafeNormalize(Vector2.Zero) * segmentHeight;
                distanceLeft = drawPosition.Distance(NPC.Center);
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
