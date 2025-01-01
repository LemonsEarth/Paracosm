using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.TheNameless
{
    [AutoloadBossHead]
    public class TheNameless : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        public float Attack
        {
            get { return NPC.ai[1]; }
            private set
            {
                int diffMod = -1; // One less attack if not in expert
                if (Main.expertMode)
                {
                    diffMod = 0;
                }
                int maxVal = phase == 1 ? 2 : 3;
                if (value > maxVal + diffMod || value < 0)
                {
                    NPC.ai[1] = 0;
                }
                else
                {
                    NPC.ai[1] = value;
                }
            }
        }
        ref float AttackTimer => ref NPC.ai[2];
        ref float AttackCount => ref NPC.ai[3];

        int AttackTimer2 = 0;
        int AttackCount2 = 0;

        bool phase2FirstTime = false;
        int phase { get; set; } = 1;
        bool phaseTransition = false;

        float attackDuration = 0;
        int[] attackDurations = { 480, 480, 900 };
        int[] attackDurations2 = { 600, 900, 720, 900 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;
        Vector2 arenaCenter = Vector2.Zero;
        bool arenaFollow = true;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()},
            {"SplitSpear", ModContent.ProjectileType<VoidSpearSplit>()},
            {"Bomb", ModContent.ProjectileType<VoidBombHostile>()},
        };

        public enum Attacks
        {
            SplitSpearThrow,
            RainingSplitShot,
            BombsWithSpear
        }

        public enum Attacks2
        {

        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            Main.npcFrameCount[NPC.type] = 3;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() { };
            /*{
                PortraitScale = 0.2f,
                PortraitPositionYOverride = -150
            };*/
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 50;
            NPC.height = 56;
            NPC.Opacity = 1;
            NPC.lifeMax = 1500000;
            NPC.defense = 100;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.value = 10000000;
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
            NPC.damage = (int)(NPC.damage * balance * 0.5f);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPosition.X);
            writer.Write(targetPosition.Y);
            writer.Write(attackDuration);
            writer.Write(phase);
            writer.Write(phase2FirstTime);
            writer.Write(AttackTimer2);
            writer.Write(AttackCount2);
            writer.Write(arenaCenter.X);
            writer.Write(arenaCenter.Y);
            writer.Write(arenaFollow);
            writer.Write(Spheres.Count);
            writer.Write(phaseTransition);
            writer.Write(NPC.Opacity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPosition.X = reader.ReadSingle();
            targetPosition.Y = reader.ReadSingle();
            attackDuration = reader.ReadSingle();
            phase = reader.ReadInt32();
            phase2FirstTime = reader.ReadBoolean();
            AttackTimer2 = reader.ReadInt32();
            AttackCount2 = reader.ReadInt32();
            arenaCenter.X = reader.ReadSingle();
            arenaCenter.Y = reader.ReadSingle();
            arenaFollow = reader.ReadBoolean();
            int count = reader.ReadInt32();
            int sphereCounter = 0;
            Spheres.Clear();
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == Proj["Sphere"])
                {
                    Spheres.Add(proj);
                    sphereCounter++;
                    if (sphereCounter >= count)
                    {
                        break;
                    }
                }
            }
            phaseTransition = reader.ReadBoolean();
            NPC.Opacity = reader.ReadSingle();
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }
            player = Main.player[NPC.target];
            playerDirection = player.Center - NPC.Center;

            DespawnCheck();

            //Visuals
            if (AITimer == 0)
            {
                NPC.Opacity = 0;
            }

            Visuals();

            if (AITimer > INTRO_DURATION - 150)
            {
                Arena();
            }

            if (AITimer < INTRO_DURATION)
            {
                Intro();
                AITimer++;
                return;
            }
            if (!phaseTransition)
            {
                NPC.dontTakeDamage = false;
            }

            if (NPC.life <= (NPC.lifeMax * 0.5f) && !phase2FirstTime)
            {
                SwitchAttacks();
                phase2FirstTime = true;
                phase = 2;
            }

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.SplitSpearThrow:
                        SplitSpearThrow();
                        break;
                    case (int)Attacks.RainingSplitShot:
                        RainingSplitShot();
                        break;
                    case (int)Attacks.BombsWithSpear:
                        BombsWithSpear();
                        break;
                }
            }
            else
            {
                switch (Attack)
                {

                }
            }

            attackDuration--;
            AITimer++;
        }

        void DespawnCheck()
        {
            if (player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000)
            {
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.8f;
        }

        void Visuals()
        {
            Lighting.AddLight(NPC.Center, 10, 10, 10);

            if (playerDirection.X != 0 && !playerDirection.HasNaNs())
            {
                NPC.spriteDirection = -Math.Sign(playerDirection.X);
            }

            //foreach (var p in Main.player)
            //{
            //    p.moonLordMonolithShader = true;
            //}
        }

        const int INTRO_DURATION = 300;
        void Intro()
        {
            NPC.dontTakeDamage = true;
            NPC.velocity = NPC.Center.DirectionTo(player.Center + new Vector2(500, 0)) * (NPC.Center.Distance(player.Center + new Vector2(500, 0)) / 12);
            NPC.Opacity += 1f / 20f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
        }

        void SwitchAttacks()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Attack++;
                if (phase == 1) attackDuration = attackDurations[(int)Attack];
                else attackDuration = attackDurations2[(int)Attack];

                AttackCount = 0;
                AttackCount2 = 0;
                AttackTimer = 0;
                AttackTimer2 = 0;
                NPC.Opacity = 1f;
                foreach (var proj in Proj)
                {
                    if (proj.Key != "Sphere")
                        DeleteProjectiles(proj.Value);
                }
            }

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
            NPC.netUpdate = true;
        }

        void ThrowSplitSpear(int timeToFire, int splitInterval, int splitCount)
        {
            var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["SplitSpear"], NPC.damage, 1f, ai0: timeToFire, ai1: splitInterval, ai2: splitCount);
            var spear = (VoidSpearSplit)proj.ModProjectile;
            spear.NPCOwner = NPC.whoAmI;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncProjectile, number: proj.whoAmI);
            }
        }

        const int SST_ATTACK_RATE = 60;
        const int SST_SPLIT_INTERVAL = 10;
        const int SST_SPLIT_COUNT = 1;
        void SplitSpearThrow()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 4;
            switch (AttackTimer)
            {
                case SST_ATTACK_RATE:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        ThrowSplitSpear(SST_ATTACK_RATE, SST_SPLIT_INTERVAL, SST_SPLIT_COUNT);
                    }
                    break;
                case 0:
                    AttackTimer = SST_ATTACK_RATE;
                    return;
            }
            AttackTimer--;
        }

        const int RSS_ATTACK_RATE1 = 120;
        const int RSS_ATTACK_RATE2 = 10;
        const int RSS_ATTACK_COUNT = 6;
        const int RSS_SPLIT_COUNT = 2;
        void RainingSplitShot()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
            switch (AttackTimer)
            {
                case 0:
                    if (AttackCount >= RSS_ATTACK_COUNT)
                    {
                        AttackTimer = RSS_ATTACK_RATE1;
                        AttackCount = 0;
                    }
                    else
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center - Vector2.UnitY * 400, Vector2.UnitY * 7, ModContent.ProjectileType<VoidBoltSplit>(), NPC.damage, 1f, ai0: 60, ai1: RSS_SPLIT_COUNT);
                        }
                        AttackCount++;
                        AttackTimer = RSS_ATTACK_RATE2;
                    }
                    return;
            }
            AttackTimer--;
        }

        const int BOMBS_CD = 120;
        void BombsWithSpear()
        {
            NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 2;
            switch (AttackTimer)
            {
                case 0:
                    AttackTimer = BOMBS_CD;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 pos = player.Center + Main.rand.NextVector2Circular(300, 300);
                            int direction = -1;
                            if (pos.Y > player.Center.Y)
                            {
                                direction = 1;
                            }
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.UnitY * Main.rand.Next(10, 25) * direction, Proj["Bomb"], NPC.damage, 1f, ai0: 60, ai1: 8, ai2: -direction);
                        }                     
                    }
                    AttackCount++;
                    if (AttackCount == 3)
                    {
                        ThrowSplitSpear(60, 5, 0);
                        AttackCount = 0;
                    }
                    return;
            }
            AttackTimer--;
        }

        void MoveToPos(Vector2 pos, float xAccel = 1f, float yAccel = 1f, float xSpeed = 1f, float ySpeed = 1f)
        {
            Vector2 direction = NPC.Center.DirectionTo(pos);
            if (direction.HasNaNs())
            {
                return;
            }
            float XaccelMod = Math.Sign(direction.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(direction.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2(XaccelMod * xAccel + xSpeed * Math.Sign(direction.X), YaccelMod * yAccel + ySpeed * Math.Sign(direction.Y));
        }

        const int BASE_ARENA_DISTANCE = 1500;
        public void Arena()
        {
            float targetArenaDistance = BASE_ARENA_DISTANCE;
            arenaFollow = true;

            if (phase == 1)
            {
                /*switch (Attack)
                {
                    case (int)Attacks.FlameBeamCombo:
                        targetArenaDistance = FB_COMBO_ARENA_DISTANCE;
                        arenaFollow = false;
                        break;
                }*/
            }
            else
            {
                /*switch (Attack)
                {

                }*/
            }
            if (AITimer % 5 == 0)
            {
                NPC.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Spheres.Count < 40)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 90f);

                        Spheres.Add(sphere);
                    }
                }
            }
            NPC.netUpdate = true;

            if (arenaFollow)
            {
                arenaCenter = NPC.Center;
            }

            for (int i = 0; i < Spheres.Count; i++)
            {
                Vector2 pos = arenaCenter + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)).RotatedBy(MathHelper.ToRadians(AITimer));
                if (Spheres[i].type != Proj["Sphere"])
                {
                    continue;
                }

                Spheres[i].velocity = (pos - Spheres[i].Center).SafeNormalize(Vector2.Zero) * (Spheres[i].Center.Distance(pos) / 50);
                Spheres[i].timeLeft = 180;
            }

            arenaDistance += ((targetArenaDistance - arenaDistance) / 60);

            foreach (var player in Main.ActivePlayers)
            {
                if (arenaCenter.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > INTRO_DURATION + 30)
                {
                    player.AddBuff(ModContent.BuffType<Infected>(), 2);
                }

                if (arenaCenter.Distance(player.MountedCenter) < arenaDistance + 50 && AITimer > INTRO_DURATION - 60)
                {
                    player.AddBuff(ModContent.BuffType<NebulousPower>(), 2);
                }
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

        public override bool? CanFallThroughPlatforms()
        {
            return true;
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

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<UnstableNebulousFlame>(), 1, 4, 8));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.FragmentNebula, 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NebulaMasterBossBag>()));
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void OnKill()
        {
            DeleteProjectiles(Proj["Sphere"]);
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedTheNameless, -1);
            for (int i = 0; i < 16; i++)
            {
                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(0, NPC.width), Main.rand.Next(0, NPC.height)), new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(2f, 5f));
            }
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