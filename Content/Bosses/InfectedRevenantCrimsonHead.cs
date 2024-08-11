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
using System.IO;

namespace Paracosm.Content.Bosses
{
    public class InfectedRevenantCrimsonHead : ModNPC
    {
        private const string NeckTexturePath = "Paracosm/Content/Bosses/InfectedRevenantCrimsonNeck";
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
        ref float Attack => ref NPC.ai[1];
        public InfectedRevenantBody body;
        ref float AttackCount => ref NPC.ai[2];
        Vector2 defaultHeadPos = Vector2.Zero;

        float attackDuration = 0;
        ref float AttackTimer => ref NPC.ai[3];
        int[] attackDurations = { 200, 600, 315, 210, 450 };

        int attackPause = 60;

        int phase => body.phase;


        enum Attacks
        {
            ichorShower,
            IchorRain,
            BloodBlasts,
            BloodBlastBurst,
            IchorSnipe,
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
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
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
            writer.Write(AITimer);
            writer.Write(AttackTimer);
            writer.Write(attackDuration);
            writer.Write(positionReached);
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
            positionReached = reader.ReadBoolean();
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
            InfectedRevenantBody body = (InfectedRevenantBody)bodyNPC.ModNPC;
            this.body = body;
            NPC.alpha = bodyNPC.alpha;
            defaultHeadPos = body.CrimsonHeadPos - new Vector2(0, 120);

            Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

            if (AITimer < 60)
            {
                NPC.frame.Y = 0;
                AITimer++;
                return;
            }

            if (body.phaseTransition)
            {
                attackDuration = 600;
                if (body.transitionDuration == 300)
                {
                    AttackOrder.Clear();
                    Attack = 5;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        AttackOrder.Enqueue(5);
                        while (AttackOrder.Count < 5)
                        {
                            int attackNum = Main.rand.Next(5, 10);
                            if (!AttackOrder.Contains(attackNum))
                            {
                                AttackOrder.Enqueue(attackNum);
                            }
                        }
                        NPC.netUpdate = true;
                    }
                }
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
                        var dust = Dust.NewDustDirect(NPC.Center, 2, 2, DustID.OrangeTorch, Scale: 3f);
                        dust.velocity = new Vector2(0, 10).RotatedBy(MathHelper.PiOver4 * i);
                        dust.noGravity = true;
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
                    case (float)Attacks.IchorSnipe:
                        IchorSnipe();
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
            AttackCount = 0;
            if (phase == 1)
            {
                AttackTimer = 60;
            }
            else AttackTimer = 0;
            positionReached = false;
        }


        const int IchorShowerCD = 15;
        void IchorShower()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (attackDuration == attackDurations[(int)Attack])
            {
                SoundEngine.PlaySound(SoundID.Zombie27 with { MaxInstances = 2, Pitch = -0.3f });
            }
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (float i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * AttackCount, -1) * 10, ProjectileID.GoldenShowerHostile, (int)(NPC.damage * 0.8f), 1);
                    }

                    AttackTimer = IchorShowerCD;
                    AttackCount += 0.05f;
                }
            }

            AttackTimer--;
        }

        bool positionReached = false;

        void IchorRain()
        {
            Vector2 RightPosition = body.NPC.Center + new Vector2(1200, -1200);
            Vector2 LeftPosition = body.NPC.Center + new Vector2(-1200, -1200);

            switch (AttackTimer)
            {
                case >= 0:
                    NPC.velocity = (RightPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(RightPosition) / 12;
                    break;
                case > -30:
                    NPC.velocity = Vector2.Zero;
                    break;
                case <= -30:
                    if (NPC.Center.Distance(LeftPosition) < 50)
                    {
                        positionReached = true;
                        break;
                    }
                    if (!positionReached)
                    {
                        NPC.velocity = new Vector2(-20, 0);
                        if (AttackTimer % 10 == 0)
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
            AttackTimer--;
        }

        const int BloodBlastCD = 60;

        void BloodBlasts()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 1, ai2: body.player.whoAmI);

                }
                AttackTimer = BloodBlastCD;
            }

            AttackTimer--;
        }

        const int BloodBlastBurstCD = 30;
        const int BloodBlastBurstCD2 = 60;

        void BloodBlastBurst()
        {
            Vector2 position = defaultHeadPos + new Vector2(30 * (float)Math.Sin(MathHelper.ToRadians(AITimer)), 50 * (float)Math.Sin(MathHelper.ToRadians(AITimer)));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;

            if (AttackTimer <= 0)
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
                    AttackTimer = BloodBlastBurstCD2;
                }
                else
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver4) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 0);
                        }

                        AttackTimer = BloodBlastBurstCD;
                    }
                }
                AttackCount++;
            }

            AttackTimer--;
        }

        const int IchorSnipeCD1 = 30;
        const int IchorSnipeCD2 = 60;

        void IchorSnipe()
        {
            if (attackDuration == attackDurations[(int)Attack])
            {
                SoundEngine.PlaySound(SoundID.Zombie8 with { MaxInstances = 2, Pitch = -0.3f });
            }
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 12;

            if (AttackTimer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 12, ProjectileID.GoldenShowerHostile, (int)(NPC.damage * 0.8f), 1);
                    
                }
                NPC.velocity -= (body.player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 10;
                AttackCount++;
                if (AttackCount > 0 && AttackCount % 3 == 0)
                {
                    AttackTimer = IchorSnipeCD2;
                }
                else
                {
                    AttackTimer = IchorSnipeCD1;
                }
            }
            AttackTimer--;
        }

        void SoaringBulletHell()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 2;
            AttackTimer = 0;
        }

        const int DashingSpamCD = 65;
        void DashingSpam()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 8;
            if (AttackTimer == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * 0.1f, -1) * 12, ProjectileID.GoldenShowerHostile, (int)(NPC.damage * 0.8f), 1);
                    }
                }
                AttackTimer = DashingSpamCD;
            }
            AttackTimer--;
        }


        const int CTBloodBlastCD = 50;
        void CorruptTorrent()
        {
            Vector2 position = defaultHeadPos + new Vector2(0, -50).RotatedBy(MathHelper.ToRadians(-AITimer));
            NPC.velocity = (position - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(position) / 12;
            if (AttackTimer == 0)
            {
                AttackTimer = CTBloodBlastCD;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1).RotatedBy(i * MathHelper.PiOver2) * 10, ModContent.ProjectileType<BloodBlast>(), (int)(NPC.damage * 0.8f), 1, ai1: 0);
                    }
                }
            }
            AttackTimer--;
        }


        const int SpiritWavesCD = 60;
        void SpiritWaves()
        {
            NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -50).RotatedBy(i * MathHelper.PiOver2), Vector2.Zero, ModContent.ProjectileType<DivineSpiritFlame>(), (int)(NPC.damage * 0.8f), 1, ai0: 20, ai1: 7, ai2: body.player.whoAmI);
                    }
                }
                AttackTimer = SpiritWavesCD;
            }
            AttackTimer--;
        }

        const int FlameChaseCD = 6;

        void FlameChase()
        {
            Vector2 botLeftPos = body.CrimsonHeadPos + new Vector2(-45, 140);
            Vector2 botRightPos = body.CrimsonHeadPos + new Vector2(45, 140);
            Vector2 topRightPos = defaultHeadPos + new Vector2(45, -20);

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
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)).RotatedBy(MathHelper.PiOver4) * 160f, ModContent.ProjectileType<DivineFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
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
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)).RotatedBy(MathHelper.ToRadians(AttackCount)) * 160f, ModContent.ProjectileType<DivineFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
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
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)) * 160f, ModContent.ProjectileType<DivineFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (topRightPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(topRightPos) / 6;
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
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(i * 0.6f * (MathHelper.PiOver4 / 4)) * 160f, ModContent.ProjectileType<DivineFlamethrower>(), (int)(NPC.damage * 0.8f), 1);
                            }
                        }
                        AttackTimer = FlameChaseCD;
                    }
                    AttackTimer--;
                    NPC.velocity = (defaultHeadPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(defaultHeadPos) / 6;
                    break;
            }
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