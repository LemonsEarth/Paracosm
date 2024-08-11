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
using Microsoft.Build.Tasks;
using Terraria.Chat;
using Terraria.Localization;
using System.Threading;

namespace Paracosm.Content.Bosses
{
    public class InfectedRevenantCorruptHead : ModNPC
    {
        private const string NeckTexturePath = "Paracosm/Content/Bosses/InfectedRevenantCorruptNeck";
        private static Asset<Texture2D> NeckTexture;

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
        public InfectedRevenantBody body;

        Vector2 defaultHeadPos = Vector2.Zero;

        public float attackDuration = 0;
        public ref float AttackTimer => ref NPC.ai[3];
        int[] attackDurations = { 300, 300, 360, 400, 600 };

        int attackPause = 60;

        int phase => body.phase;

        enum Attacks
        {
            CursedBurstFire,
            SpinCursedCross,
            CursedCircles,
            CursedWalls,
            BiteExplosion,
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
            NPC.width = 58;
            NPC.height = 52;
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
            foreach (int attack in AttackOrder)
            {
                writer.Write(attack);
            }
            writer.Write(AITimer);
            writer.Write(AttackTimer);
            writer.Write(attackDuration);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AttackOrder.Clear();
            for (int i = 0; i < 5; i++)
            {
                AttackOrder.Enqueue(reader.ReadInt32());
            }
            AITimer = reader.ReadSingle();
            AttackTimer = reader.ReadSingle();
            attackDuration = reader.ReadSingle();
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

        public override bool CheckActive()
        {
            return false;
        }

        public override void AI()
        {
            NPC bodyNPC = Main.npc[ParentIndex];
            if (Main.netMode != NetmodeID.MultiplayerClient && (Main.npc[(int)ParentIndex] == null || !Main.npc[(int)ParentIndex].active || Main.npc[(int)ParentIndex].type != BodyType()))
            {
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                return;
            }
            InfectedRevenantBody body = (InfectedRevenantBody)bodyNPC.ModNPC;
            this.body = body;
            NPC.alpha = bodyNPC.alpha;
            defaultHeadPos = body.CorruptHeadPos - new Vector2(0, 120);
            if (AITimer < 60)
            {
                NPC.frame.Y = 0;
                AITimer++;
                return;
            }

            if (body.phaseTransition)
            {
                if (body.transitionDuration > 150)
                {
                    NPC.velocity = ((defaultHeadPos + new Vector2(0, 20)) - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(defaultHeadPos + new Vector2(0, 20)) / 36);
                    AttackTimer = 60;
                }
                else
                {
                    NPC.velocity = ((defaultHeadPos - new Vector2(0, 20)) - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(defaultHeadPos - new Vector2(0, 20)) / 12);
                    AttackTimer = 20;
                }
                return;
            }

            if (phase == 1)
            {
                if (attackDuration <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        ChooseAttack();
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        var dust = Dust.NewDustDirect(NPC.Center, 2, 2, DustID.CursedTorch, Scale: 3f);
                        dust.velocity = new Vector2(0, 10).RotatedBy(MathHelper.PiOver4 * i);
                        dust.noGravity = true;
                    }
                }

                switch (Attack)
                {
                    case (float)Attacks.CursedBurstFire:
                        CursedBurstFire();
                        break;
                    case (float)Attacks.SpinCursedCross:
                        SpinCursedCross();
                        break;
                    case (float)Attacks.CursedCircles:
                        CursedCircles();
                        break;
                    case (float)Attacks.CursedWalls:
                        RisingCursedWall();
                        break;
                    case (float)Attacks.BiteExplosion:
                        BiteExplosion();
                        break;
                }
            }

            else if (phase == 2)
            {
                Attack = body.Attack;

                switch (Attack)
                {
                    case (int)InfectedRevenantBody.Attacks.SoaringBulletHell:
                        SoaringBulletHell();
                        break;
                    case (int)InfectedRevenantBody.Attacks.DashingSpam:
                        DashingSpam();
                        break;
                    case (int)InfectedRevenantBody.Attacks.CorruptTorrent:
                        CorruptTorrent();
                        break;
                    case (int)InfectedRevenantBody.Attacks.SpiritWaves:
                        SpiritWaves();
                        break;
                    case (int)InfectedRevenantBody.Attacks.FlameChase:
                        FlameChase();
                        break;
                }
            }



            attackDuration--;
            AITimer++;
        }

        void ChooseAttack()
        {
            NPC.frame.Y = 0;
            ResetVars();
            Attack = AttackOrder.Dequeue();
            AttackOrder.Enqueue((int)Attack);
            attackDuration = attackDurations[(int)Attack] + attackPause;
            NPC.netUpdate = true;
        }

        public void ResetVars()
        {
            NPC.frame.Y = 0;
            if (phase == 1)
            {
                AttackTimer = 60;
            }
            else AttackTimer = 0;
            AttackCount = 0;
        }

        const int CursedBurstCD1 = 20;
        const int CursedBurstCD2 = 60;
        int cursedBurstCount = 0;

        void CursedBurstFire()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 10, ProjectileID.CursedFlameHostile, (int)(NPC.damage * 0.8f), 10);
                    NPC.velocity -= (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 10;
                    cursedBurstCount++;
                    AttackTimer = CursedBurstCD1;
                    if (cursedBurstCount >= 4)
                    {
                        cursedBurstCount = 0;
                        AttackTimer = CursedBurstCD2;
                    }
                }
            }

            AttackTimer--;
        }

        const int CursedCrossCD = 45;

        void SpinCursedCross()
        {
            Vector2 position = defaultHeadPos + new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(AITimer));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver2) * 20, ProjectileID.CursedFlameHostile, (int)(NPC.damage * 0.8f), 10);
                    }
                    AttackTimer = CursedCrossCD;

                }
            }
            AttackTimer--;
        }

        const int CursedCirclesCD = 60;

        void CursedCircles()
        {
            NPC.velocity = (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * (50 / (NPC.Center.Distance(defaultHeadPos) + 1));

            if (AttackTimer == 0 && AttackCount < 4)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * (MathHelper.PiOver2 / 3)) * 4, ModContent.ProjectileType<CursedSpiritFlame>(), (int)(NPC.damage * 0.8f), 1, ai0: 30, ai1: body.player.MountedCenter.X - NPC.Center.X, ai2: body.player.MountedCenter.Y - NPC.Center.Y);
                        CursedSpiritFlame CursedSpiritFlame = (CursedSpiritFlame)proj.ModProjectile;
                        CursedSpiritFlame.speed = 10;
                    }
                }
            }

            Lighting.AddLight(NPC.Center, 0.5f, 0f, 1f);

            if (AttackTimer <= -30)
            {
                AttackCount++;
                AttackTimer = CursedCirclesCD;
            }
            if (AttackCount >= 4)
            {
                attackDuration -= 10;
            }

            AttackTimer--;
        }

        const int CursedWallCD = 5;

        void RisingCursedWall()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;
            var dust = Dust.NewDustDirect(NPC.position, NPC.width, 120, DustID.CursedTorch, SpeedY: -20, Scale: 3f);
            dust.noGravity = true;

            if (AttackTimer == 0 && AttackCount < 60)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i += 2)
                    {
                        var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), body.NPC.Center + new Vector2(i * 1200, 1200 - (AttackCount * 30)), Vector2.Zero, ModContent.ProjectileType<CursedSpiritFlame>(), (int)(NPC.damage * 0.8f), 1, ai0: 5, ai1: -i, ai2: 0);
                        CursedSpiritFlame CursedSpiritFlame = (CursedSpiritFlame)proj.ModProjectile;
                        CursedSpiritFlame.speed = 40;
                    }
                }
            }

            if (AttackTimer <= -5)
            {
                AttackTimer = CursedWallCD;
                AttackCount++;
            }

            if (AttackCount >= 60)
            {
                attackDuration -= 10;
            }

            AttackTimer--;
        }

        const int BiteExplosionCD = 120;

        void BiteExplosion()
        {
            if (AttackTimer < -10)
            {
                NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(defaultHeadPos) / 12);
            }
            else
            {
                NPC.velocity = (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * (NPC.Center.Distance(body.player.MountedCenter) / 24);
            }

            if (AttackTimer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver4) * 20, ProjectileID.CursedFlameHostile, (int)(NPC.damage * 0.8f), 10);
                    }
                }
            }

            if (AttackTimer <= -60)
            {
                AttackTimer = BiteExplosionCD;
            }
            AttackTimer--;
        }

        void SoaringBulletHell()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 2;
            AttackTimer = 0;
        }

        const int DashingSpamCD = 60;
        void DashingSpam()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
            if (AttackTimer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * (MathHelper.PiOver4 / 2)) * 20, ProjectileID.CursedFlameHostile, (int)(NPC.damage * 0.8f), 1);
                    }
                }
                AttackTimer = DashingSpamCD;
            }
            AttackTimer--;
        }

        const int FlamethrowerCD = 33;
        void CorruptTorrent()
        {
            Vector2 position = defaultHeadPos + new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(AITimer * 1.6f));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;
            if (AttackTimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { MaxInstances = 2, PitchVariance = 1.0f });
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)) * 3f * AttackCount, ModContent.ProjectileType<CursedFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                    }
                    if (AttackCount < 30)
                    {
                        AttackCount += 3;
                    }
                }
                AttackTimer = FlamethrowerCD - AttackCount;
            }
            AttackTimer--;
        }

        const int cursedSpiritCD = 9;
        const int cursedSpiritCD2 = 45;

        void SpiritWaves()
        {
            Vector2 position = defaultHeadPos + new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(AITimer * 1.6f));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;
            Vector2 neckToHead = body.CorruptHeadPos.DirectionTo(defaultHeadPos);
            Vector2 neckToPlayer = body.CorruptHeadPos.DirectionTo(body.player.MountedCenter);
            if (Math.Abs(AngleBetween(neckToHead, neckToPlayer)) < 60)
            {
                if (AttackTimer <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 8, ModContent.ProjectileType<CursedSpiritFlame>(), (int)(NPC.damage * 0.8f), 1, ai0: 60 - (AttackCount * cursedSpiritCD), ai1: body.player.MountedCenter.X - NPC.Center.X, ai2: body.player.MountedCenter.Y - NPC.Center.Y);
                        CursedSpiritFlame CursedSpiritFlame = (CursedSpiritFlame)proj.ModProjectile;
                        CursedSpiritFlame.speed = 60;
                        AttackCount++;
                    }
                    if (AttackCount <= 6)
                    {
                        AttackTimer = cursedSpiritCD;
                    }
                    else
                    {
                        AttackTimer = cursedSpiritCD2;
                        AttackCount = 0;
                    }
                }
            }
            AttackTimer--;
        }

        const int FlameChaseCD = 6;

        void FlameChase()
        {
            Vector2 botLeftPos = body.CorruptHeadPos + new Vector2(-45, 140);
            Vector2 botRightPos = body.CorruptHeadPos + new Vector2(45, 140);
            Vector2 topLeftPos = defaultHeadPos + new Vector2(-45, -20);

            switch (body.AttackDuration)
            {
                case > 660:
                    AttackTimer = FlameChaseCD;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
                    break;
                case > 540:
                    if (AttackTimer <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { MaxInstances = 2, PitchVariance = 1.0f });
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)).RotatedBy(MathHelper.PiOver4) * 160f, ModContent.ProjectileType<CursedFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (botLeftPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(botLeftPos) / 6;
                    break;
                case > 480:
                    AttackTimer = FlameChaseCD;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
                    AttackCount = -60;
                    break;
                case > 360:
                    if (AttackTimer <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { MaxInstances = 2, PitchVariance = 1.0f });
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)).RotatedBy(MathHelper.ToRadians(AttackCount)) * 160f, ModContent.ProjectileType<CursedFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackCount += 9;
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (botRightPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(botRightPos) / 6;
                    break;
                case > 300:
                    AttackTimer = FlameChaseCD;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
                    break;
                case > 180:
                    if (AttackTimer <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { MaxInstances = 2, PitchVariance = 1.0f });
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(-1, 0).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)) * 160f, ModContent.ProjectileType<CursedFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (topLeftPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(topLeftPos) / 6;
                    break;
                case > 120:
                    AttackTimer = FlameChaseCD;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6; ;
                    break;
                case > 0:
                    if (AttackTimer <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { MaxInstances = 2, PitchVariance = 1.0f });
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(-1, 0).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)) * 160f, ModContent.ProjectileType<CursedFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
                    break;
            }
        }

        float AngleBetween(Vector2 u, Vector2 v)
        {
            return (float)Math.Acos(Vector2.Dot(u, v) / (u.Length() * v.Length()));
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 12;
            int endFrame = 2;

            if (AttackTimer > 24 || AttackTimer < 0)
            {
                NPC.frame.Y = 0;
                return;
            }
            if (AttackTimer < 5)
            {
                NPC.frame.Y = 2 * frameHeight;
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
                spriteBatch.Draw(NeckTexture.Value, drawPosition - screenPos, null, new Color(255 - (drawnSegments * 10), 255 - (drawnSegments * 10), 255 - (drawnSegments * 10), 255 - NPC.alpha), rotation, new Vector2(NeckTexture.Value.Width / 2f, NeckTexture.Value.Height / 2f), 1f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
