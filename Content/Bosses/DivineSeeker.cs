using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using SteelSeries.GameSense;
using Terraria.Audio;
using Paracosm.Content.Projectiles.Hostile;
using Terraria.ModLoader.Default;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using System.IO;
using Paracosm.Content.Items.BossBags;


namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class DivineSeeker : ModNPC
    {
        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];

        int damage;

        int frame = 0;
        float idleTimer = 0;
        Vector2 randomIdlePoint = Vector2.Zero;
        float AITimerA = 0;
        bool isCircling = false;
        float dashTime = 0;
        float AITimerB = 0;
        float AITimerC = 0;
        bool isDashing = false;
        bool spinning = false;
        float timeBeforeDash = 45;
        Vector2 ChosenPosition = Vector2.Zero;
        float circleWaitTime = 60;
        float circlingTime = 0;
        int randDirection = 1;
        Vector2 tempPlayerDir = Vector2.Zero;

        Vector2 shootDirection = new Vector2(1, 1);
        float speen = 0;
        bool destroyedProj1 = false;
        bool destroyedProj2 = false;
        bool rage = false;
        Vector2 dashFireDir = Vector2.Zero;

        float indicatorTimer;
        Vector2 tempPlayerCenter = Vector2.Zero;


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 128;
            NPC.height = 126;
            NPC.lifeMax = 40000;
            NPC.defense = 30;
            NPC.damage = 80;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 100000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 6;
            damage = NPC.damage;
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SeveredSpace");
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * 0.65f);
            NPC.damage = (int)(NPC.damage * balance);
            NPC.defense = 30;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override bool CheckDead()
        {
            Filters.Scene.Deactivate("DivineSeekerShader");
            return true;
        }

        int phase = 1;
        int attack = 1;
        bool p2FirstTime = false;
        Vector2 shootOffset;
        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (NPC.life > NPC.lifeMax / 2)
            {
                phase = 1;
            }
            else if (NPC.life <= NPC.lifeMax / 2 && NPC.life > NPC.lifeMax / 8)
            {
                phase = 2;
            }
            else
            {
                phase = 3;
            }
            if (player.dead == true || Main.dayTime == true)
            {
                NPC.active = false;
            }
            AITimer++;
            Vector2 playerDirection = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            shootOffset = (playerDirection).SafeNormalize(Vector2.Zero) * new Vector2(NPC.width / 2, NPC.height / 2).Length();
            if (!isDashing && !spinning)
            {
                NPC.rotation = playerDirection.ToRotation() - MathHelper.PiOver2;
            }

            NPC.TargetClosest();

            Vector2 topLeft = player.position + new Vector2(-600, -300);
            Vector2 topRight = player.position + new Vector2(600, -300);
            Vector2 botLeft = player.position + new Vector2(-600, 300);
            Vector2 botRight = player.position + new Vector2(600, 300);

            Vector2[] cornerPositions = { topLeft, topRight, botLeft, botRight };

            Vector2 targetPosition = topLeft;

            if (AITimer % 30 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GemSapphire, Main.rand.Next(1, 1), Main.rand.Next(1, 1), Scale: 1);
                }
            }

            if (!Filters.Scene["DivineSeekerShader"].IsActive())
            {
                Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(152, 152, 255));
            }

            switch (phase)
            {
                case 1:
                    AITimerA++;
                    if (AITimerA > 30 && AITimerA < 720)
                    {
                        attack = 1;
                        isDashing = false;
                        timeBeforeDash = 45;
                        dashTime = 0;
                        isCircling = false;
                        circlingTime = 0;
                        circleWaitTime = 60;
                        indicatorTimer = 0;
                    }
                    else if (AITimerA >= 750 && AITimerA < 1110)
                    {
                        attack = 2;
                    }
                    else if (AITimerA >= 1140 && AITimerA < 1500)
                    {
                        attack = 3;
                    }
                    else if (AITimerA >= 1530 && AITimerA < 1890)
                    {
                        attack = 4;
                    }
                    else if (AITimerA >= 1920 && AITimerA < 2280)
                    {
                        attack = 5;
                    }
                    else
                    {
                        attack = 0;
                    }
                    if (attack == 0)
                    {
                        NPC.damage = 0;
                        if (idleTimer == 1)
                        {
                            randomIdlePoint = Vector2.Zero;
                            idleTimer = 30;
                        }
                        idleTimer--;
                        if (randomIdlePoint == Vector2.Zero)
                        {
                            randomIdlePoint = player.Center + new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300));
                        }
                        if (idleTimer > 1 && randomIdlePoint != Vector2.Zero)
                        {
                            NPC.velocity = (randomIdlePoint - NPC.Center).SafeNormalize(Vector2.Zero) * randomIdlePoint.Distance(NPC.Center) / 10;
                        }
                    }
                    if (attack == 1)
                    {
                        NPC.damage = 0;
                        if (AITimerA >= 30 && AITimerA <= 180)
                        {
                            targetPosition = topLeft;
                        }
                        if (AITimerA > 180 && AITimerA <= 360)
                        {
                            targetPosition = topRight;
                        }
                        if (AITimerA > 360 && AITimerA <= 540)
                        {
                            targetPosition = botRight;
                        }
                        if (AITimerA > 540 && AITimerA <= 720)
                        {
                            targetPosition = botLeft;
                        }

                        NPC.velocity = (targetPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(targetPosition) / 10;
                        if (AITimerA % 30 == 0 && NPC.Center.Distance(targetPosition) < 200)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection * 30, ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                        }
                        if (AITimerA % 5 == 0 && NPC.Center.Distance(targetPosition) >= 200)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlueFire>(), damage / 4, 3);
                        }
                    }

                    if (attack == 2)
                    {
                        NPC.damage = 80;
                        Vector2 RightPosition = player.Center + new Vector2(600, 0);
                        Vector2 LeftPosition = player.Center + new Vector2(-600, 0);
                        timeBeforeDash--;
                        if (timeBeforeDash > 10)
                        {
                            if (NPC.Center.Distance(LeftPosition) < NPC.Center.Distance(RightPosition))
                            {
                                ChosenPosition = LeftPosition;
                            }
                            else
                            {
                                ChosenPosition = RightPosition;
                            }
                        }

                        if (isDashing == false)
                        {
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 6;
                        }
                        else
                        {
                            dashTime++;
                            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                            if (dashTime % 4 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                            }
                        }

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            dashFireDir = new Vector2(0, Math.Sign(playerDirection.Y));
                            NPC.velocity = new Vector2(60 * Math.Sign(playerDirection.X), 0);
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0 });
                        }

                        if (dashTime == 30)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity *= 50;
                                }
                            }
                        }
                        if (dashTime >= 45)
                        {
                            isDashing = false;
                            timeBeforeDash = 45;
                            NPC.velocity = Vector2.Zero;
                            ChosenPosition = Vector2.Zero;
                            dashTime = 0;
                        }
                    }

                    if (attack == 3)
                    {
                        NPC.damage = 0;
                        Vector2 TopPosition = player.position + new Vector2(0, -400);
                        circleWaitTime--;

                        if (isCircling == false)
                        {
                            NPC.velocity = (TopPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(TopPosition) / 6;
                        }
                        else
                        {
                            NPC.Center = player.Center + new Vector2(0, -450).RotatedBy(circlingTime / 10 * randDirection);
                            circlingTime++;
                            if ((Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-30) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(30)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(60) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(120)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(150) || Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-150)) || (Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-60) && Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-120)))
                            {
                                if (circlingTime % 8 == 0)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 6, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                                }
                            }
                        }
                        if (circleWaitTime == 0)
                        {
                            isCircling = true;
                        }
                    }

                    if (attack == 4)
                    {
                        NPC.damage = 80;
                        if (ChosenPosition == Vector2.Zero)
                        {
                            if (ChosenPosition == Vector2.Zero)
                            {
                                foreach (var position in cornerPositions)
                                {
                                    if (NPC.Center.Distance(position) < NPC.Center.Distance(ChosenPosition))
                                    {
                                        ChosenPosition = position;
                                    }
                                }
                                timeBeforeDash = 75;
                            }
                        }
                        timeBeforeDash--;

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            NPC.velocity = tempPlayerDir * 70;
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.2f });
                        }

                        if (timeBeforeDash > 15)
                        {
                            tempPlayerDir = playerDirection;
                        }

                        if (isDashing == false)
                        {
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 10;
                        }
                        else
                        {
                            dashTime++;
                            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                            if (dashTime % 8 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30, -30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 40, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-30, 30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                            }
                        }

                        if (dashTime > 0)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity = (player.Center - proj.Center).SafeNormalize(Vector2.Zero) * dashTime / 6;
                                }
                            }
                        }
                        if (dashTime >= 45)
                        {
                            isDashing = false;
                            timeBeforeDash = 75;
                            NPC.velocity = Vector2.Zero;
                            ChosenPosition = Vector2.Zero;
                            dashTime = 0;
                        }


                    }

                    if (attack == 5)
                    {
                        NPC.damage = 0;
                        if (indicatorTimer == 0 || indicatorTimer == 120 || indicatorTimer == 240)
                        {
                            tempPlayerCenter = player.Center;
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 200), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 1; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 200), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                        }

                        if (indicatorTimer == 60 || indicatorTimer == 180 || indicatorTimer == 300)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 200), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 1; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 200), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                        }

                        if (NPC.Center.Distance(player.Center) > 200)
                        {
                            NPC.velocity = playerDirection * NPC.Center.Distance(player.Center) / 30;
                        }
                        else
                        {
                            NPC.velocity *= 0.5f;
                        }
                        indicatorTimer++;
                    }

                    if (AITimerA > 2280)
                    {

                        AITimerA = 0;
                    }

                    break;
                case 2:
                    AITimerB++;
                    if (p2FirstTime == false)
                    {
                        SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = -0.2f, Volume = 0.95f });
                        p2FirstTime = true;
                    }
                    if (AITimerB > 30 && AITimerB < 330)
                    {
                        attack = 1;
                        isDashing = false;
                        timeBeforeDash = 30;
                        dashTime = 0;
                        isCircling = false;
                        circlingTime = 0;
                        circleWaitTime = 60;
                        indicatorTimer = 0;
                    }
                    else if (AITimerB > 390 && AITimerB <= 720)
                    {
                        attack = 2;
                    }
                    else if (AITimerB > 750 && AITimerB <= 1110)
                    {
                        attack = 3;
                    }
                    else if (AITimerB > 1140 && AITimerB <= 1500)
                    {
                        attack = 4;
                    }

                    else if (AITimerB > 1530 && AITimerB <= 1890)
                    {
                        attack = 5;
                    }

                    else
                    {
                        attack = 0;
                        rage = false;
                        NPC.velocity = new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20));
                    }

                    if (attack == 1)
                    {
                        NPC.damage = 0;
                        if (AITimerB >= 0 && AITimerB <= 60)
                        {
                            rage = false;
                            targetPosition = topLeft;
                        }
                        if (AITimerB > 60 && AITimerB <= 120)
                        {
                            targetPosition = topRight;
                        }
                        if (AITimerB > 120 && AITimerB <= 180)
                        {
                            targetPosition = botRight;
                        }
                        if (AITimerB > 180 && AITimerB <= 210)
                        {
                            targetPosition = botLeft;
                        }
                        if (AITimerB >= 210 && AITimerB <= 240)
                        {
                            rage = true;
                            targetPosition = topLeft;
                        }
                        if (AITimerB > 240 && AITimerB <= 270)
                        {
                            targetPosition = topRight;
                        }
                        if (AITimerB > 270 && AITimerB <= 300)
                        {
                            targetPosition = botRight;
                        }
                        if (AITimerB > 300 && AITimerB <= 330)
                        {
                            targetPosition = botLeft;

                        }
                        NPC.velocity = (targetPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(targetPosition) / 10;

                        if (AITimerB % 10 == 0 && rage == false && NPC.Center.Distance(targetPosition) < 200)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 25, ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection.RotatedBy(-MathHelper.PiOver4) * 15, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection.RotatedBy(MathHelper.PiOver4) * 15, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                        }
                        if (NPC.Center.Distance(targetPosition) >= 200)
                        {
                            if (AITimerB % 8 == 0 && rage == true)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlueFire>(), damage / 4, 3);
                            }
                        }
                    }
                    if (attack == 2)
                    {
                        NPC.damage = 80;
                        Vector2 RightPosition = player.position + new Vector2(600, 0);
                        Vector2 LeftPosition = player.position + new Vector2(-600, 0);
                        timeBeforeDash--;

                        if (timeBeforeDash > 10)
                        {
                            if (NPC.Center.Distance(LeftPosition) < NPC.Center.Distance(RightPosition))
                            {
                                ChosenPosition = LeftPosition;
                            }
                            else
                            {
                                ChosenPosition = RightPosition;
                            }
                        }

                        if (isDashing == false)
                        {

                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 10;
                        }
                        else
                        {
                            dashTime++;
                            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                            if (dashTime % 2 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                            }
                        }

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            dashFireDir = new Vector2(0, Math.Sign(playerDirection.Y));
                            NPC.velocity = new Vector2(100 * Math.Sign(playerDirection.X), 0);
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0 });
                        }

                        if (dashTime == 25)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity *= 50;
                                }
                            }
                        }
                        if (dashTime >= 30)
                        {
                            isDashing = false;
                            timeBeforeDash = 30;
                            NPC.velocity = Vector2.Zero;
                            ChosenPosition = Vector2.Zero;
                            dashTime = 0;
                        }
                    }

                    if (attack == 3)
                    {
                        isDashing = false;
                        timeBeforeDash = 45;
                        dashTime = 0;
                        NPC.damage = 0;
                        Vector2 TopPosition = player.position + new Vector2(0, -400);
                        circleWaitTime--;

                        if (isCircling == false)
                        {
                            NPC.velocity = (TopPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(TopPosition) / 6;
                        }
                        else
                        {
                            NPC.Center = player.Center + new Vector2(0, -450).RotatedBy(circlingTime / 9 * randDirection);
                            circlingTime++;
                            if ((Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-30) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(30)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(60) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(120)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(150) || Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-150)) || (Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-60) && Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-120)))
                            {
                                if (circlingTime % 6 == 0)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 4, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                                }
                            }
                        }
                        if (circleWaitTime == 0)
                        {
                            isCircling = true;
                        }
                    }

                    if (attack == 4)
                    {
                        NPC.damage = 80;
                        if (ChosenPosition == Vector2.Zero)
                        {
                            foreach (var position in cornerPositions)
                            {
                                if (NPC.Center.Distance(position) < NPC.Center.Distance(ChosenPosition))
                                {
                                    ChosenPosition = position;
                                }
                            }
                            timeBeforeDash = 45;
                        }
                        timeBeforeDash--;

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            NPC.velocity = tempPlayerDir * 70;
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.05f });
                        }

                        if (timeBeforeDash > 10)
                        {
                            tempPlayerDir = playerDirection;
                        }

                        if (isDashing == false)
                        {
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 7;
                        }
                        else
                        {
                            dashTime++;
                            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                            if (dashTime % 8 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30, -30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-30, 30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 3);
                            }
                        }

                        if (dashTime > 0)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity = (player.Center - proj.Center).SafeNormalize(Vector2.Zero) * dashTime / 3;
                                }
                            }
                        }
                        if (dashTime >= 35)
                        {
                            isDashing = false;
                            timeBeforeDash = 45;
                            NPC.velocity = Vector2.Zero;
                            ChosenPosition = Vector2.Zero;
                            dashTime = 0;
                        }
                    }

                    if (attack == 5)
                    {
                        NPC.damage = 0;
                        if (indicatorTimer == 0 || indicatorTimer == 120 || indicatorTimer == 240)
                        {
                            tempPlayerCenter = player.Center;
                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 150), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 1; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 150), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }

                            if (indicatorTimer != 0)
                            {
                                for (int i = 0; i < 20; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                                }
                                for (int i = 0; i < 20; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                                }
                            }
                        }

                        if (indicatorTimer == 60 || indicatorTimer == 180 || indicatorTimer == 300)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 1; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                        }

                        if (indicatorTimer == 359)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                        }

                        if (NPC.Center.Distance(player.Center) > 200)
                        {
                            NPC.velocity = playerDirection * NPC.Center.Distance(player.Center) / 30;
                        }
                        else
                        {
                            NPC.velocity *= 0.5f;
                        }
                        indicatorTimer++;
                    }

                    if (AITimerB > 1890)
                    {
                        AITimerB = 0;
                    }

                    break;
                case 3:
                    if (AITimerC == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = -0.4f });
                        NPC.Center = player.position + new Vector2(0, -300);
                    }
                    AITimerC++;
                    if (AITimerC <= 360)
                    {
                        indicatorTimer = 0;
                        spinning = true;
                        isDashing = false;
                        attack = 1;
                    }
                    else if (AITimerC > 360 && AITimerC < 720)
                    {
                        attack = 2;
                    }

                    if (attack == 1)
                    {
                        NPC.velocity = Vector2.Zero;
                        speen += 1;
                        if (AITimerC < 120 && AITimerC % 20 == 0)
                        {
                            NPC.rotation = (shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                        }

                        if (AITimerC >= 120 && AITimerC % 2 == 0)
                        {
                            NPC.rotation = (shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                        }
                        if (AITimerC >= 120 && AITimerC % 30 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 1).RotatedBy(MathHelper.PiOver2 * i) * 4, ModContent.ProjectileType<BlueFireBall>(), damage / 4, 2);
                            }

                        }
                    }

                    else if (attack == 2)
                    {
                        spinning = false;
                        indicatorTimer++;
                        NPC.Center = player.position + new Vector2(0, -300);
                        if (indicatorTimer == 1)
                        {
                            tempPlayerCenter = player.Center;
                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 150), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 1; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 150), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                            }
                        }
                        if (indicatorTimer == 45)
                        {

                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 1; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 4, 3);
                            }
                        }

                        if (indicatorTimer == 75)
                        {
                            indicatorTimer = 0;
                        }
                    }

                    if (AITimerC == 750)
                    {
                        AITimerC = 0;
                    }
                    break;
            }


        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frame * frameHeight;
            if (AITimer % 10 == 0)
            {
                frame++;
                if (frame == 3)
                {
                    frame = 0;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Parashard>(), 1, 20, 30));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DivineSeekerBossBag>()));
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }
    }
}
