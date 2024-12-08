using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Hostile;
using System.IO;
using Terraria;
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
        public ref float GunCount => ref NPC.ai[1];
        ref float AttackTimer => ref NPC.ai[3];
        int AttackCount = 0;
        float randNum = 0;
        float AITimer = 0;
        float attackDuration = 0;
        Vector2 shootDirection = Vector2.Zero;
        Vector2 playerDirection => NPC.Center.DirectionTo(body.player.Center);
        VortexMothership body;

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
            NPC.damage = 40;
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

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.damage = (int)(NPC.damage * balance * 0.4f);
        }

        public static int BodyType()
        {
            return ModContent.NPCType<VortexMothership>();
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
            body = (VortexMothership)bodyNPC.ModNPC;
            NPC.Opacity = body.NPC.Opacity;
            if (shootDirection == Vector2.Zero) shootDirection = playerDirection; // By default, face the player
            NPC.rotation = shootDirection.ToRotation() + MathHelper.PiOver2;
            NPC.Center = body.NPC.Center + body.gunOffsets[(int)GunCount];
            if (AITimer < 60)
            {
                AITimer++;
                return;
            }

            if (body.phase == 1)
            {
                switch (body.Attack)
                {
                    case (int)VortexMothership.Attacks.TeslashotSpam:
                        TeslaShotSpam();
                        break;
                    case (int)VortexMothership.Attacks.CenterBlast:
                        CenterBlast();
                        break;
                    case (int)VortexMothership.Attacks.PredictiveShots:
                        PredictiveShots();
                        break;
                    case (int)VortexMothership.Attacks.Mix:
                        Mix();
                        break;
                }
            }
            else if (body.phase == 2)
            {
                switch (body.Attack)
                {
                    case (int)VortexMothership.Attacks2.ChillTeslaShots:
                        ChillTeslaShots();
                        break;
                    case (int)VortexMothership.Attacks2.MineSpam:
                        MineSpam();
                        break;
                    case (int)VortexMothership.Attacks2.Lasers:
                        Lasers();
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        public void SwitchAttacks(int attack)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                AttackTimer = 0;
                AttackCount = 0;
                randNum = 0;
                if ((int)body.Attack == (int)VortexMothership.Attacks2.Lasers)
                {
                    AttackTimer = 30;
                }
            }
            NPC.netUpdate = true;
        }

        const int TESLA_SHOT_CD = 30;
        void TeslaShotSpam()
        {
            if (AttackTimer <= 0 - AttackCount) // Slightly randomized fire rate
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackCount = Main.rand.Next(-10, 2);
                    shootDirection = playerDirection;
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 10, ModContent.ProjectileType<TeslaShot>(), NPC.damage, 1, ai1: 2f);
                }
                AttackTimer = TESLA_SHOT_CD;
            }
            AttackTimer--;
        }

        void CenterBlast()
        {
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    shootDirection = NPC.Center.DirectionTo(body.NPC.Center);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<TeslaCore>(), NPC.damage, 1, ai0: body.NPC.Center.X, ai1: body.NPC.Center.Y, ai2: GunCount);
                }
                AttackTimer = 2; // Only fire once
            }
        }

        const int PREDICTIVE_SHOT_CD1 = 5;
        const int PREDICTIVE_SHOT_CD2 = 20;
        void PredictiveShots()
        {
            if (GunCount < 2) // 2 bottom guns
            {
                if (AttackTimer <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        shootDirection = playerDirection + body.player.velocity / 6;
                        NPC.netUpdate = true;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.SafeNormalize(Vector2.Zero) * 20, ModContent.ProjectileType<TrackingTeslaShot>(), NPC.damage, 1, ai1: body.player.whoAmI);
                    }
                    if (AttackCount < 6)
                    {
                        AttackTimer = PREDICTIVE_SHOT_CD1;
                        AttackCount++;
                    }
                    else
                    {
                        AttackTimer = PREDICTIVE_SHOT_CD2 * (GunCount + 1);
                        AttackCount = 0;
                    }
                }
            }
            else // 2 top guns
            {
                if (AttackTimer <= 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        shootDirection = playerDirection;
                        NPC.netUpdate = true;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 12, ModContent.ProjectileType<TeslaShot>(), NPC.damage, 1);
                    }
                    AttackTimer = TESLA_SHOT_CD;
                }
                AttackTimer--;
            }

            AttackTimer--;
        }

        const int MIX_CENTER_BLAST_CD = 120;
        const int MIX_TESLA_SHOT_CD = 30;
        const int MIX_MINE_CD1 = 2;
        const int MIX_MINE_CD2 = 60;
        const int MIX_LASER_CD = 300;

        void Mix()
        {
            switch (GunCount)
            {
                case 0:
                    if (AttackTimer <= 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            shootDirection = playerDirection;
                            NPC.netUpdate = true;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<TeslaCore>(), NPC.damage, 1, ai0: NPC.Center.X + shootDirection.X * 20, ai1: NPC.Center.Y + shootDirection.Y * 20, ai2: GunCount);
                        }
                        AttackTimer = MIX_CENTER_BLAST_CD;
                    }
                    AttackTimer--;
                    break;
                case 1:
                    if (AttackTimer <= 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            shootDirection = playerDirection;
                            NPC.netUpdate = true;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 10, ModContent.ProjectileType<TeslaShot>(), NPC.damage, 1);
                        }
                        AttackTimer = MIX_TESLA_SHOT_CD;
                    }
                    AttackTimer--;
                    break;
                case 2:
                    if (AttackTimer <= 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            randNum = Main.rand.NextFloat(30, 60);
                            shootDirection = Vector2.UnitY.RotatedBy(22.5f * AttackCount);
                            NPC.netUpdate = true;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.SafeNormalize(Vector2.Zero) * randNum, ModContent.ProjectileType<VortexMine>(), NPC.damage, 1);
                        }
                        if (AttackCount < 16)
                        {
                            AttackTimer = MIX_MINE_CD1;
                            AttackCount++;
                        }
                        else
                        {
                            AttackTimer = MIX_MINE_CD2;
                            AttackCount = 0;
                        }
                    }
                    AttackTimer--;
                    break;
                case 3:
                    if (AttackTimer == 30)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            shootDirection = playerDirection;
                            NPC.netUpdate = true;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 50);
                        }
                    }
                    if (AttackTimer <= 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<VortexLaser>(), NPC.damage * 2, 1, ai0: 50, ai1: 0);
                        }
                        AttackTimer = MIX_LASER_CD;
                    }
                    AttackTimer--;
                    break;
            }
        }

        const int CHILL_TESLA_SHOT_CD = 120;
        void ChillTeslaShots()
        {
            if (AttackTimer <= 0 - AttackCount) // Slightly randomized fire rate
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackCount = Main.rand.Next(-10, 2);
                    shootDirection = playerDirection;
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 10, ModContent.ProjectileType<TeslaShot>(), NPC.damage, 1, ai1: 2f);
                }
                AttackTimer = CHILL_TESLA_SHOT_CD;
            }
            AttackTimer--;
        }

        const int MINE_SPAM_CD1 = 15;
        const int MINE_SPAM_CD2 = 60;
        void MineSpam()
        {
            if (AttackTimer <= 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    randNum = Main.rand.NextFloat(30, 60);
                    shootDirection = playerDirection;
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(randNum * 3)) * randNum, ModContent.ProjectileType<VortexMine>(), NPC.damage / 2, 1);
                }
                if (AttackCount < 8)
                {
                    AttackTimer = MINE_SPAM_CD1;
                    AttackCount++;
                }
                else
                {
                    AttackTimer = MINE_SPAM_CD2 * (GunCount + 1);
                    AttackCount = 0;
                }
            }
            AttackTimer--;
        }

        void Lasers()
        {
            if (AttackTimer == 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    shootDirection = playerDirection;
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 50);
                }
            }
            if (AttackTimer <= 0 - AttackCount)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackCount = Main.rand.Next(-10, 5);
                    NPC.netUpdate = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<VortexLaser>(), NPC.damage * 2, 1, ai0: 50, ai1: 0);
                }
                AttackTimer = 300;
            }
            AttackTimer--;
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

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    }
}
