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
using Paracosm.Content.Projectiles;
using Paracosm.Common.Systems;
using Terraria.GameContent.Bestiary;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Ranged;
using Paracosm.Content.Items.Weapons.Summon;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;


namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class DivineSeeker : ModNPC
    {
        public ref float AITimer => ref NPC.ai[0];

        int damage;

        float idleTimer = 0;
        float AITimerA = 0;
        bool isCircling = false;
        float dashTime = 0;
        float AITimerB = 0;
        float AITimerC = 0;
        bool isDashing = false;
        bool spinning = false;
        float timeBeforeDash = 45;
        Vector2 ChosenPosition
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }
        float circleWaitTime = 60;
        float circlingTime = 0;
        Vector2 tempPlayerDir = Vector2.Zero;

        Vector2 shootDirection = new Vector2(1, 1);
        float speen = 0;
        bool rage = false;
        Vector2 dashFireDir = Vector2.Zero;

        float indicatorTimer;
        Vector2 tempPlayerCenter = Vector2.Zero;


        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers drawMod = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionYOverride = -15f,
                PortraitScale = 0.8f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawMod);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("A monster who arrived from a different world. Though feral in nature, it possesses high intelligence, suggesting its arrival was not without purpose."),
            }); 
        }
        

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 128;
            NPC.height = 126;
            NPC.lifeMax = 45000;
            NPC.defense = 30;
            NPC.damage = 80;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 100000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);
            damage = NPC.damage;

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SeveredSpace");
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.65f);
            NPC.damage = (int)(NPC.damage * balance);
            NPC.defense = 30;
        }

        public override void OnSpawn(IEntitySource source)
        {
            phase = 1;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override bool CheckDead()
        {
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("DivineSeekerShader");
            return true;
        }

        float phase = 1;
        ref float Attack => ref NPC.ai[1];
        bool p2FirstTime = false;
        Vector2 shootOffset;
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            if (player.dead || NPC.Center.Distance(player.MountedCenter) > 2500)
            {
                NPC.EncourageDespawn(300);
            }
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
            Vector2 playerDirection = (player.MountedCenter - NPC.Center).SafeNormalize(Vector2.Zero);
            shootOffset = (playerDirection).SafeNormalize(Vector2.Zero) * new Vector2(NPC.width / 2, NPC.height / 2).Length();
            if (!isDashing && !spinning)
            {
                NPC.rotation = playerDirection.ToRotation() - MathHelper.PiOver2;
            }

            Vector2 topLeft = player.position + new Vector2(-600, -300);
            Vector2 topRight = player.position + new Vector2(600, -300);
            Vector2 botLeft = player.position + new Vector2(-600, 300);
            Vector2 botRight = player.position + new Vector2(600, 300);

            Vector2[] cornerPositions = { topLeft, topRight, botLeft, botRight };

            Vector2 targetPosition = topLeft;



            if (!Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(152, 152, 255));
            }

            switch (phase)
            {
                case 1:
                    if (AITimer % 30 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shiverthorn, Main.rand.Next(-10, 10), Main.rand.Next(-10, 10), Scale: 1);
                        }
                    }
                    AITimerA++;
                    if (AITimerA > 30 && AITimerA < 720)
                    {
                        Attack = 1;
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
                        Attack = 2;
                    }
                    else if (AITimerA >= 1140 && AITimerA < 1500)
                    {
                        Attack = 3;
                    }
                    else if (AITimerA >= 1530 && AITimerA < 1890)
                    {
                        Attack = 4;
                    }
                    else if (AITimerA >= 1920 && AITimerA < 2280)
                    {
                        Attack = 5;
                    }
                    else
                    {
                        Attack = 0;
                    }
                    if (Attack == 0)
                    {
                        NPC.damage = 0;
                        if (idleTimer == 1)
                        {
                            ChosenPosition = Vector2.Zero;
                            idleTimer = 30;
                        }
                        idleTimer--;
                        if (ChosenPosition == Vector2.Zero && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            ChosenPosition = player.MountedCenter + new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300));
                            NPC.netUpdate = true;
                        }
                        if (idleTimer > 1 && ChosenPosition != Vector2.Zero)
                        {
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ChosenPosition.Distance(NPC.Center) / 20;
                        }
                    }
                    if (Attack == 1)
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
                        if (AITimerA % 30 == 0 && NPC.Center.Distance(targetPosition) < 200 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection * 30, ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            NPC.netUpdate = true;
                        }
                        if (AITimerA % 5 == 0 && NPC.Center.Distance(targetPosition) >= 200 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlueFire>(), damage / 2, 3, ai1: 5);
                            NPC.netUpdate = true;
                        }
                    }

                    if (Attack == 2)
                    {
                        NPC.damage = 80;
                        Vector2 RightPosition = player.MountedCenter + new Vector2(600, 0);
                        Vector2 LeftPosition = player.MountedCenter + new Vector2(-600, 0);
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
                            if (dashTime % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                NPC.netUpdate = true;
                            }
                        }

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            dashFireDir = new Vector2(0, Math.Sign(playerDirection.Y));
                            NPC.velocity = new Vector2(60 * Math.Sign(playerDirection.X), 0);
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0 }, NPC.Center);
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

                    if (Attack == 3)
                    {
                        NPC.damage = 0;
                        Vector2 TopPosition = player.position + new Vector2(0, -450);
                        circleWaitTime--;

                        if (isCircling == false)
                        {
                            NPC.velocity = (TopPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(TopPosition) / 6;
                        }
                        else
                        {
                            NPC.Center = player.MountedCenter + new Vector2(0, -450).RotatedBy(circlingTime / 10);
                            circlingTime++;
                            if ((Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-30) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(30)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(60) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(120)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(150) || Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-150)) || (Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-60) && Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-120)))
                            {
                                if (circlingTime % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 6, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                    NPC.netUpdate = true;
                                }
                            }
                        }
                        if (circleWaitTime == 0)
                        {
                            isCircling = true;
                        }
                    }

                    if (Attack == 4)
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
                            timeBeforeDash = 75;
                        }
                        timeBeforeDash--;

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            NPC.velocity = tempPlayerDir * 70;
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.2f }, NPC.Center);
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
                            if (dashTime % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30, -30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-30, 30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                NPC.netUpdate = true;
                            }
                        }

                        if (dashTime > 0)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity = (player.MountedCenter - proj.Center).SafeNormalize(Vector2.Zero) * dashTime / 6;
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

                    if (Attack == 5)
                    {
                        NPC.damage = 0;
                        if (indicatorTimer == 0 || indicatorTimer == 120 || indicatorTimer == 240)
                        {
                            tempPlayerCenter = player.MountedCenter;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 200), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);
                                }
                                for (int i = 1; i < 8; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 200), new Vector2(40, 0), ModContent.ProjectileType<IndicatorLaser>(), 0, 3);

                                }
                                NPC.netUpdate = true;
                            }
                        }

                        if ((indicatorTimer == 60 || indicatorTimer == 180 || indicatorTimer == 300) && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 200), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            }
                            for (int i = 1; i < 8; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 200), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);

                            }
                            NPC.netUpdate = true;
                        }

                        if (NPC.Center.Distance(player.MountedCenter) > 200)
                        {
                            NPC.velocity = playerDirection * NPC.Center.Distance(player.MountedCenter) / 30;
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
                    if (AITimer % 30 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shiverthorn, Main.rand.Next(-10, 10), Main.rand.Next(-10, 10), Scale: 1.5f);
                        }
                    }
                    damage = 100;
                    if (AITimerB == 0)
                    {
                        foreach (var proj in Main.ActiveProjectiles)
                        {
                            if (proj.hostile == true)
                            {
                                proj.Kill();
                            }
                        }
                    }
                    AITimerB++;
                    if (p2FirstTime == false)
                    {
                        SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = -0.2f, Volume = 0.95f }, NPC.Center);

                        p2FirstTime = true;
                    }
                    if (AITimerB > 30 && AITimerB < 330)
                    {
                        Attack = 1;
                        isDashing = false;
                        timeBeforeDash = 30;
                        dashTime = 0;
                        isCircling = false;
                        circlingTime = 0;
                        circleWaitTime = 60;
                        indicatorTimer = 0;
                    }
                    else if (AITimerB > 390 && AITimerB <= 750)
                    {
                        Attack = 2;
                    }
                    else if (AITimerB > 780 && AITimerB <= 1110)
                    {
                        Attack = 3;
                    }
                    else if (AITimerB > 1140 && AITimerB <= 1500)
                    {
                        Attack = 4;
                    }

                    else if (AITimerB > 1530 && AITimerB <= 1920)
                    {
                        Attack = 5;
                    }

                    else
                    {
                        Attack = 0;
                    }

                    if (Attack == 0)
                    {
                        NPC.damage = 0;
                        if (idleTimer == 1)
                        {
                            ChosenPosition = Vector2.Zero;
                            idleTimer = 30;
                        }
                        idleTimer--;
                        if (ChosenPosition == Vector2.Zero && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            ChosenPosition = player.MountedCenter + new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(200, 300));
                            NPC.netUpdate = true;
                        }
                        if (idleTimer > 1 && ChosenPosition != Vector2.Zero)
                        {
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ChosenPosition.Distance(NPC.Center) / 20;
                        }
                    }

                    if (Attack == 1)
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

                        if (AITimerB % 10 == 0 && rage == false && NPC.Center.Distance(targetPosition) < 200 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 25, ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection.RotatedBy(-MathHelper.PiOver4) * 15, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection.RotatedBy(MathHelper.PiOver4) * 15, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                            NPC.netUpdate = true;
                        }
                        if (NPC.Center.Distance(targetPosition) >= 200)
                        {
                            if (AITimerB % 8 == 0 && rage == true && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlueFire>(), damage / 2, 3, ai1: 5);
                                NPC.netUpdate = true;
                            }
                        }
                    }
                    if (Attack == 2)
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
                            if (dashTime % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -dashFireDir, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                NPC.netUpdate = true;
                            }
                        }

                        if (timeBeforeDash == 0)
                        {
                            isDashing = true;
                            dashFireDir = new Vector2(0, Math.Sign(playerDirection.Y));
                            NPC.velocity = new Vector2(100 * Math.Sign(playerDirection.X), 0);
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0 }, NPC.Center);
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

                    if (Attack == 3)
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
                            NPC.Center = player.MountedCenter + new Vector2(0, -450).RotatedBy(circlingTime / 9);
                            circlingTime++;
                            if ((Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-30) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(30)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(60) && Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(120)) || (Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(150) || Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-150)) || (Math.Atan2(playerDirection.Y, playerDirection.X) < MathHelper.ToRadians(-60) && Math.Atan2(playerDirection.Y, playerDirection.X) > MathHelper.ToRadians(-120)))
                            {
                                if (circlingTime % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection * 4, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                    NPC.netUpdate = true;
                                }
                            }
                        }
                        if (circleWaitTime == 0)
                        {
                            isCircling = true;
                        }
                    }

                    if (Attack == 4)
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
                            SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = 0.05f }, NPC.Center);
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
                            if (dashTime % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30, -30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-30, 30), Vector2.Zero, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 3);
                                NPC.netUpdate = true;
                            }
                        }

                        if (dashTime > 0)
                        {
                            foreach (Projectile proj in Main.ActiveProjectiles)
                            {
                                if (proj.type == ModContent.ProjectileType<BlueFireBall>())
                                {
                                    proj.velocity = (player.MountedCenter - proj.Center).SafeNormalize(Vector2.Zero) * dashTime / 3;
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

                    if (Attack == 5)
                    {
                        if (AITimerB < 1560)
                        {
                            ChosenPosition = player.position + new Vector2(0, -500);
                            NPC.velocity = (ChosenPosition - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(ChosenPosition) / 10;
                        }
                        else if (AITimerB >= 1620)
                        {
                            NPC.velocity = playerDirection * 7;
                            if (Main.netMode != NetmodeID.MultiplayerClient && AITimerB % 30 == 0)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection * 20, ModContent.ProjectileType<ParacosmicFlameHostile>(), damage / 2, 0);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection * 15, ModContent.ProjectileType<ParacosmicFlameHostile>(), damage / 2, 0);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + shootOffset, playerDirection * 10, ModContent.ProjectileType<ParacosmicFlameHostile>(), damage / 2, 0);
                                for (int i = 0; i < 8; i++)
                                {
                                    Vector2 pos = player.MountedCenter + new Vector2(800, -800).RotatedBy(i * MathHelper.PiOver4);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, (player.MountedCenter - pos).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 0);

                                }
                                NPC.netUpdate = true;
                            }
                        }
                        else
                        {
                            NPC.velocity = Vector2.Zero;
                        }
                    }

                    if (AITimerB > 1950)
                    {
                        AITimerB = 0;
                    }

                    break;
                case 3:
                    if (AITimerC == 0)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shiverthorn, Main.rand.Next(-30, 30), Main.rand.Next(-30, 30), Scale: 2f);
                        }
                        SoundEngine.PlaySound(SoundID.Roar with { MaxInstances = 0, Pitch = -0.4f }, NPC.Center);
                        NPC.Center = player.position + new Vector2(0, -300);
                        foreach (var proj in Main.ActiveProjectiles)
                        {
                            if (proj.hostile == true)
                            {
                                proj.Kill();
                            }
                        }
                    }
                    if (AITimer % 30 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shiverthorn, Main.rand.Next(-10, 10), Main.rand.Next(-10, 10), Scale: 1.5f);
                        }
                    }
                    AITimerC++;
                    if (AITimerC <= 360)
                    {
                        indicatorTimer = 0;
                        spinning = true;
                        isDashing = false;
                        Attack = 1;
                    }
                    else if (AITimerC > 360 && AITimerC < 720)
                    {
                        Attack = 2;
                    }

                    if (Attack == 1)
                    {
                        NPC.velocity = Vector2.Zero;
                        speen += 1;
                        if (AITimerC < 120 && AITimerC % 20 == 0)
                        {
                            NPC.rotation = (shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 5, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            NPC.netUpdate = true;
                        }

                        if (AITimerC >= 120 && AITimerC % 2 == 0)
                        {
                            NPC.rotation = (shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10).ToRotation();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -shootDirection.RotatedBy(MathHelper.ToRadians(speen)).RotatedBy(MathHelper.PiOver2) * 10, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                            NPC.netUpdate = true;
                        }
                        if (AITimerC >= 120 && AITimerC % 50 == 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 1).RotatedBy(MathHelper.PiOver2 * i) * 4, ModContent.ProjectileType<BlueFireBall>(), damage / 3, 2);
                                NPC.netUpdate = true;
                            }

                        }
                    }

                    else if (Attack == 2)
                    {
                        spinning = false;
                        indicatorTimer++;
                        NPC.Center = player.position + new Vector2(0, -300);
                        if (indicatorTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            tempPlayerCenter = player.MountedCenter;
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
                            NPC.netUpdate = true;
                        }
                        if (indicatorTimer == 45 && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            }
                            for (int i = 1; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-1200, -i * 150), new Vector2(40, 0), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            }
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), tempPlayerCenter + new Vector2(-i * 150, -1000), new Vector2(0, 40), ModContent.ProjectileType<BlueLaser>(), damage / 2, 3);
                            }
                            NPC.netUpdate = true;
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
            int frameDur = 10;
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

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Parashard>(), 1, 20, 30));
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicFlames>(), 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<ParashardSword>(), ModContent.ItemType<ParacosmicFurnace>(), ModContent.ItemType<GravityBarrage>(), ModContent.ItemType<ParacosmicEyeStaff>()));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DivineSeekerBossBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.DivineSeekerRelic>()));
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

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedDivineSeeker, -1);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!isDashing)
            {
                return true;
            }
            Texture2D texture = TextureAssets.Npc[Type].Value;

            // Redraw the projectile with the color not influenced by light
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}