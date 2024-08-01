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
    public class InfectedRevenantCrimsonHead : ModNPC
    {
        private const string NeckTexturePath = "Paracosm/Content/Bosses/InfectedRevenantCrimsonNeck";
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
        int[] attackDurations = { 200, 210, 200, 210, 200 };


        const int ichorShowerCD = 20;

        enum Attacks
        {
            ichorShower,
            IchorRain,
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
            defaultHeadPos = body.CrimsonHeadPos - new Vector2(0, 120);
            if (attackDuration == 0)
            {
                offset = 0;
                attackTimer = 0;
                Attack = AttackOrder.Dequeue();
                AttackOrder.Enqueue((int)Attack);
                attackDuration = attackDurations[(int)Attack];
                SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.2f });
            }

            switch (Attack)
            {
                case (float)Attacks.ichorShower:
                    IchorShower();
                    break;
                case (float)Attacks.IchorRain:
                    IchorRain();
                    break;
                case (float)Attacks.Attack3:
                    IchorShower();
                    break;
                case (float)Attacks.Attack4:
                    IchorRain();
                    break;
                case (float)Attacks.Attack5:
                    IchorShower();
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
            Vector2 randomPos = body.CrimsonHeadPos + new Vector2(Main.rand.Next(-150, 30), Main.rand.Next(-300, 30));

            return randomPos;
        }


        float offset = 0;
        void IchorShower()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (attackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (float i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * offset, -1) * 10, ProjectileID.GoldenShowerHostile, 50, 1);
                    }
                    NPC.netUpdate = true;
                    attackTimer = ichorShowerCD;
                    offset += 0.2f;
                }
            }
            attackTimer--;
        }

        void IchorRain()
        {
            Vector2 position = body.NPC.Center + new Vector2(1200, -1200);

            switch (attackTimer)
            {
                case < 60:
                    NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;
                    break;
                case < 90:
                    NPC.velocity = Vector2.Zero;
                    break;
                case 180:
                    SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.4f });
                    break;
                case < 180:
                    NPC.velocity = new Vector2(-20, 0);
                    if (AITimer % 10 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 10), ProjectileID.GoldenShowerHostile, 50, 1);
                            NPC.netUpdate = true;
                        }
                    }
                    break;
                case >= 180:
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;
                    break;
            }
            attackTimer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (body == null)
            {
                return true;
            }

            Vector2 drawPosition = body.CrimsonHeadPos;
            Vector2 NeckBaseToHead = NPC.Center - body.CrimsonHeadPos;
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
