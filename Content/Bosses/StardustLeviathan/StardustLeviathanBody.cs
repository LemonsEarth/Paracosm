using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Bosses.StardustLeviathan
{
    public class StardustLeviathanBody : ModNPC
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

        StardustLeviathanHead head;

        Vector2 bodyPlayerDir => head.player.Center - NPC.Center;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 128;
            NPC.height = 64;
            NPC.Opacity = 1;
            NPC.lifeMax = 1500000;
            NPC.defense = 160;
            NPC.damage = 120;
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
            NPC.damage = (int)(NPC.damage * balance * 0.4f);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.Opacity);
            writer.Write(RandNum);
            writer.Write(AttackCount);
            writer.Write(AttackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.Opacity = reader.ReadSingle();
            RandNum = reader.ReadSingle();
            AttackCount = reader.ReadInt32();
            AttackTimer = reader.ReadInt32();
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
                NPC.active = false;
                NPC.life = 0;
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

            if (AITimer % (SegmentNum % 4) == 0)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GemTopaz, Scale: Main.rand.NextFloat(1f, 1.2f));
                dust.noGravity = true;
            }

            if (AITimer < StardustLeviathanHead.INTRO_DURATION)
            {
                AITimer++;
                return;
            }

            if (head.phase == 1)
            {
                switch (head.Attack)
                {
                    case (int)StardustLeviathanHead.Attacks.DashingStarSpam:
                        DashingStarSpam();
                        break;
                    case (int)StardustLeviathanHead.Attacks.Circling:
                        Circling();
                        break;
                    case (int)StardustLeviathanHead.Attacks.Chasing:
                        Chasing();
                        break;
                    case (int)StardustLeviathanHead.Attacks.Minefield:
                        Minefield();
                        break;
                }
            }
            else
            {
                switch (head.Attack)
                {
                    case (int)StardustLeviathanHead.Attacks2.DashingSpam:
                        DashingSpam();
                        break;
                    case (int)StardustLeviathanHead.Attacks2.CirclingBulletHell:
                        CirclingBulletHell();
                        break;
                    case (int)StardustLeviathanHead.Attacks2.ChasingMineTrail:
                        ChasingMineTrail();
                        break;
                    case (int)StardustLeviathanHead.Attacks2.Chasing2:
                        Chasing2();
                        break;
                }
            }

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

        public void SwitchAttacks(int attack)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                AttackTimer = 0;
                AttackCount = 0;
                RandNum = 0;
                if (head.phase == 1 && head.Attack == (int)StardustLeviathanHead.Attacks.Minefield)
                {
                    NPC.Center = head.arenaCenter - Vector2.UnitX * StardustLeviathanHead.MINEFIELD_ARENA_DISTANCE;
                }
                else if (head.phase == 2 && head.Attack == (int)StardustLeviathanHead.Attacks2.CirclingBulletHell)
                {
                    NPC.Center = head.arenaCenter;
                }
            }
            NPC.netUpdate = true;
        }

        const int DASHING_ATTACK_RATE = 90;
        void DashingStarSpam()
        {
            switch (AttackTimer)
            {
                case 0:
                    if (SegmentNum % 4 != 0)
                    {
                        return;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, bodyPlayerDir.SafeNormalize(Vector2.Zero) * 20, head.Proj["Starshot"], head.NPC.damage, 1);
                    }
                    AttackTimer = DASHING_ATTACK_RATE;
                    return;
            }

            AttackTimer--;
        }

        const int CIRCLING_ATTACK_RATE = 80;
        void Circling()
        {
            switch (AttackTimer)
            {
                case 0:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        RandNum = Main.rand.Next(-100, 100);
                        NPC.netUpdate = true;
                        Vector2 pos = head.player.Center + new Vector2(RandNum, -800);
                        Vector2 posToPlayer = pos.DirectionTo(head.player.Center);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, posToPlayer * 25, head.Proj["Starshot"], head.NPC.damage, 1, ai1: 1);
                    }
                    AttackTimer = CIRCLING_ATTACK_RATE + SegmentNum % 8;
                    return;
            }

            AttackTimer--;
        }

        const int CHASING_ATTACK_RATE = 60;
        void Chasing()
        {
            switch (AttackTimer)
            {
                case 0:
                    if (Main.netMode != NetmodeID.MultiplayerClient && SegmentNum % 4 == 0)
                    {
                        for (int i = -1; i <= 1; i+=2)
                        {
                            Vector2 toFollowing = Main.npc[FollowingNPC].Center - NPC.Center;
                            Vector2 perpendicular = toFollowing.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.PiOver2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perpendicular * 10, head.Proj["Starshot"], head.NPC.damage, 1);
                        }
                    }
                    AttackTimer = CHASING_ATTACK_RATE;
                    return;
            }

            AttackTimer--;
        }

        const int MINEFIELD_ATTACK_TIMER = 90;
        const int MINEFIELD_POS_DISTANCE = 800;
        void Minefield()
        {
            if (SegmentNum % 4 == 0)
            {
                return;
            }
            switch(AttackTimer)
            {
                case 0:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        AttackCount = Main.rand.Next(0, 16);
                        RandNum = Main.rand.NextFloat(20f, 40f);

                        Vector2 pos = head.player.Center + (Vector2.UnitY * MINEFIELD_POS_DISTANCE).RotatedBy(AttackCount * (MathHelper.PiOver4 / 2));
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, pos.DirectionTo(head.player.Center) * RandNum, head.Proj["Mine"], head.NPC.damage, 1);
                    }
                    NPC.netUpdate = true;
                    AttackTimer = MINEFIELD_ATTACK_TIMER + SegmentNum;
                    return;
            }
            AttackTimer--;
        }


        const int DASHING_SPAM_ATTACK_RATE = 90;
        void DashingSpam()
        {
            switch (AttackTimer)
            {
                case 0:
                    if (SegmentNum % 8 != 0)
                    {
                        return;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, bodyPlayerDir.SafeNormalize(Vector2.Zero) * 20, head.Proj["Starshot"], head.NPC.damage, 1);
                    }
                    AttackTimer = DASHING_SPAM_ATTACK_RATE;
                    return;
            }

            AttackTimer--;
        }


        const int CIRCLING_BH_ATTACK_RATE = 25;
        void CirclingBulletHell()
        {
            switch (AttackTimer)
            {
                case 0:
                    int finalStage = head.NPC.life < head.NPC.lifeMax / 4 ? 1 : 2;
                    if (Main.netMode != NetmodeID.MultiplayerClient && SegmentNum % 4 == 0)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            Vector2 toFollowing = Main.npc[FollowingNPC].Center - NPC.Center;
                            Vector2 perpendicular = toFollowing.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.PiOver2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perpendicular * 5, head.Proj["Starshot"], head.NPC.damage, 1);
                        }
                    }
                    AttackTimer = CIRCLING_BH_ATTACK_RATE * finalStage;
                    return;
            }

            AttackTimer--;
        }

        const int MINE_TRAIL_ATTACK_TIMER = 60;
        void ChasingMineTrail()
        {
            if (SegmentNum % 4 == 0)
            {
                return;
            }
            switch (AttackTimer)
            {
                case 0:
                    int finalStage = head.NPC.life < head.NPC.lifeMax / 4 ? 1 : 2;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        AttackCount = Main.rand.Next(0, 360);
                        RandNum = Main.rand.NextFloat(10, 25);
                        Vector2 direction = (Vector2.UnitY * RandNum).RotatedBy(MathHelper.ToRadians(AttackCount));
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction, head.Proj["Mine"], head.NPC.damage, 1, ai1: 360);
                    }
                    NPC.netUpdate = true;
                    AttackTimer = MINE_TRAIL_ATTACK_TIMER * finalStage;
                    return;
            }
            AttackTimer--;
        }

        const int CHASING2_ATTACK_RATE = 60;
        void Chasing2()
        {
            if (SegmentNum != 1)
            {
                return;
            }
            switch (AttackTimer)
            {
                case 0:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 pos = head.player.Center + new Vector2(0, -800).RotatedBy(MathHelper.ToRadians(i * 72));
                            Vector2 posToPlayer = pos.DirectionTo(head.player.Center);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, posToPlayer * 20, head.Proj["Starshot"], head.NPC.damage, 1, ai1: 1);
                        }              
                    }
                    NPC.netUpdate = true;
                    AttackTimer = CHASING2_ATTACK_RATE;
                    return;
            }

            AttackTimer--;
        }


        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            projectile.damage /= 2;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type == ProjectileID.LastPrismLaser)
            {
                modifiers.FinalDamage /= 2;
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (head.phase == 1)
            {
                modifiers.FinalDamage *= 0.9f;
            }
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
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("Paracosm:ScreenTintShader");
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