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
using System.IO;

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
        ref float AttackCount => ref NPC.ai[2];
        Vector2 defaultHeadPos = Vector2.Zero;

        ref float attackDuration => ref NPC.ai[3];
        float attackTimer = 0;
        int[] attackDurations = { 200, 600, 315, 210, 200 };


        enum Attacks
        {
            ichorShower,
            IchorRain,
            BloodBlasts,
            BloodBlastBurst,
            Attack5,
        }

        Queue<int> AttackOrder = new Queue<int>();

        public override void Load()
        {
            NeckTexture = ModContent.Request<Texture2D>(NeckTexturePath);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
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
            NPC.netAlways = true;
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

        public override void SendExtraAI(BinaryWriter writer)
        {
            foreach (int attack in AttackOrder)
            {
                writer.Write(attack);
            }
            writer.Write(attackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AttackOrder.Clear();
            for (int i = 0; i < 5; i++)
            {
                AttackOrder.Enqueue(reader.ReadInt32());
            }
            attackTimer = reader.ReadSingle();
        }

        public override void AI()
        {
            NPC bodyNPC = Main.npc[ParentIndex];
            InfectedRevenantBody body = (InfectedRevenantBody)bodyNPC.ModNPC;
            if (body == null)
            {
                NPC.active = false;
                return;
            }
            this.body = body;
            defaultHeadPos = body.CrimsonHeadPos - new Vector2(0, 120);

            if (AITimer < 60)
            {
                NPC.frame.Y = 0;
                AITimer++;
                return;
            }

            if (attackDuration <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.frame.Y = 0;
                    ResetVars();
                    Attack = AttackOrder.Dequeue();
                    AttackOrder.Enqueue((int)Attack);
                    attackDuration = attackDurations[(int)Attack];
                    NPC.netUpdate = true;
                }    
            }

            switch (Attack)
            {
                case (float)Attacks.ichorShower:
                    IchorShower();
                    break;
                case (float)Attacks.IchorRain:
                    IchorRain();
                    break;
                case (float)Attacks.BloodBlasts:
                    BloodBlasts();
                    break;
                case (float)Attacks.BloodBlastBurst:
                    BloodBlastBurst();
                    break;
                case (float)Attacks.Attack5:
                    IchorShower();
                    break;
            }
            attackDuration--;
            AITimer++;
        }

        void ResetVars()
        {
            AttackCount = 0;
            attackTimer = 0;
            positionReached = false;
        }


        const int IchorShowerCD = 15;
        void IchorShower()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (attackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (float i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * AttackCount, -1) * 10, ProjectileID.GoldenShowerHostile, (int)(NPC.damage * 0.8f), 1);
                    }
                    
                    attackTimer = IchorShowerCD;
                    AttackCount += 0.05f;
                }
            }

            attackTimer--;
        }

        bool positionReached = false;

        void IchorRain()
        {
            Vector2 RightPosition = body.NPC.Center + new Vector2(1200, -1200);
            Vector2 LeftPosition = body.NPC.Center + new Vector2(-1200, -1200);

            switch (attackTimer)
            {
                case < 60:
                    NPC.velocity = (RightPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(RightPosition) / 12;
                    break;
                case < 90:
                    NPC.velocity = Vector2.Zero;
                    break;
                case > 90:
                    if (NPC.Center.Distance(LeftPosition) < 50)
                    {
                        positionReached = true;
                        break;
                    }
                    if (!positionReached)
                    {
                        NPC.velocity = new Vector2(-20, 0);
                        if (attackTimer % 10 == 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 10), ProjectileID.GoldenShowerHostile, (int)(NPC.damage * 0.8f), 1);                
                            }
                        }
                    }
                    else
                    {
                        NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;
                        if (NPC.Center.Distance(defaultHeadPos) <= 10)
                        {
                            attackDuration -= 10;
                        }
                    }
                    break;
            }
            attackTimer++;
        }

        const int BloodBlastCD = 60;

        void BloodBlasts()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (attackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 1, ai2: body.player.whoAmI);
                    
                }
                attackTimer = BloodBlastCD;
            }

            attackTimer--;
        }

        const int BloodBlastBurstCD = 30;
        const int BloodBlastBurstCD2 = 60;

        void BloodBlastBurst()
        {
            Vector2 position = defaultHeadPos + new Vector2(30 * (float)Math.Sin(MathHelper.ToRadians(AITimer)), 50 * (float)Math.Sin(MathHelper.ToRadians(AITimer)));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;

            if (attackTimer <= 0)
            {
                if (AttackCount > 0 && AttackCount % 3 == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(i * 120)) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 1, ai2: body.player.whoAmI);
                        }
                        
                    }
                    attackTimer = BloodBlastBurstCD2;
                }
                else
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver4) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 0);
                        }
                        
                        attackTimer = BloodBlastBurstCD;
                    }
                }
                AttackCount++;
            }

            attackTimer--;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 12;
            int endFrame = 2;

            if (attackTimer > 24)
            {
                NPC.frame.Y = 0;
            }

            if (NPC.frame.Y < endFrame * frameHeight)
            {
                NPC.frameCounter--;

                if (NPC.frameCounter <= 0)
                {
                    NPC.frameCounter = frameDur;
                    NPC.frame.Y += frameHeight;
                }
            }
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