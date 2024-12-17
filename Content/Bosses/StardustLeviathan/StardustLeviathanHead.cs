using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Bosses.StardustLeviathan
{
    [AutoloadBossHead]
    public class StardustLeviathanHead : ModNPC
    {
        int BodyType => ModContent.NPCType<StardustLeviathanBody>();
        int TailType => ModContent.NPCType<StardustLeviathanTail>();
        const int MAX_SEGMENT_COUNT = 40;
        int SegmentCount = 0;

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
                int maxVal = 3;
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
        public int phase { get; private set; } = 1;
        bool phaseTransition = false;

        float attackDuration = 0;
        int[] attackDurations = { 480, 480, 960, 720 };
        int[] attackDurations2 = { 600, 900, 720, 900 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;
        public Vector2 arenaCenter { get; private set; } = Vector2.Zero;
        bool arenaFollow = true;

        List<Projectile> Spheres = new List<Projectile>();
        List<StardustLeviathanBody> Segments = new List<StardustLeviathanBody>();
        public Dictionary<string, int> Proj { get; } = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()},
            {"Starshot", ModContent.ProjectileType<StarshotHostile>()},
            {"Mine", ModContent.ProjectileType<StardustEnergyMine>()},
        };

        public enum Attacks
        {
            DashingStarSpam,
            Circling,
            Chasing,
            Minefield
        }

        public enum Attacks2
        {

        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            Main.npcFrameCount[NPC.type] = 1;
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.StardustPillar,
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 174;
            NPC.height = 300;
            NPC.Opacity = 1;
            NPC.lifeMax = 1500000;
            NPC.defense = 80;
            NPC.damage = 40;
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
            writer.Write(phaseTransition);
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
            arenaCenter = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            arenaFollow = reader.ReadBoolean();
            phaseTransition = reader.ReadBoolean();
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }
            player = Main.player[NPC.target];
            playerDirection = player.Center - NPC.Center;
            if (player.dead || !player.active || NPC.Center.Distance(player.MountedCenter) > 8000)
            {
                NPC.active = false;
            }

            //Visuals
            if (AITimer == 0)
            {
                arenaCenter = NPC.Center;
                SpawnSegments();
                NPC.Opacity = 0;
            }

            if (NPC.velocity.X != 0 && !playerDirection.HasNaNs())
            {
                NPC.spriteDirection = -Math.Sign(NPC.velocity.X);
            }

            float rotmul = NPC.spriteDirection == 1 ? -MathHelper.PiOver2 : MathHelper.PiOver2;
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

            /*if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:ScreenTintShader").GetShader().UseColor(new Color(190, 255, 255));
            }*/

            Lighting.AddLight(NPC.Center, 100, 100, 100);


            foreach (var p in Main.player)
            {
                p.stardustMonolithShader = true;
            }

            if (AITimer > INTRO_DURATION)
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

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.DashingStarSpam:
                        DashingStarSpam();
                        break;
                    case (int)Attacks.Circling:
                        Circling();
                        break;
                    case (int)Attacks.Chasing:
                        Chasing();
                        break;
                    case (int)Attacks.Minefield:
                        Minefield();
                        break;
                }
            }
            else
            {

            }

            attackDuration--;
            AITimer++;
        }

        void SpawnSegments()
        {
            int latestNPC = NPC.whoAmI;
            while (SegmentCount < MAX_SEGMENT_COUNT - 2) // Body segments, excluding head and tail
            {
                latestNPC = SpawnSegment(BodyType, latestNPC);
                StardustLeviathanBody bodySegment = (StardustLeviathanBody)Main.npc[latestNPC].ModNPC;
                Segments.Add(bodySegment);
                SegmentCount++;
            }

            SpawnSegment(TailType, latestNPC);
        }

        int SpawnSegment(int type, int latestNPC)
        {
            int oldestNPC = latestNPC;
            latestNPC = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC, NPC.whoAmI, SegmentCount);

            Main.npc[oldestNPC].ai[0] = latestNPC;
            Main.npc[latestNPC].realLife = NPC.whoAmI;
            return latestNPC;
        }

        public const int INTRO_DURATION = 90;
        void Intro()
        {
            NPC.dontTakeDamage = true;
            NPC.velocity = -Vector2.UnitX * 20f;
            NPC.Opacity += 1f / 20f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
            Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].GetShader().UseProgress(AITimer / 60);
        }

        void SwitchAttacks()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Attack++;
                if (phase == 1) attackDuration = attackDurations[(int)Attack];
                else attackDuration = attackDurations2[(int)Attack];

                foreach (StardustLeviathanBody bodySegment in Segments)
                {
                    bodySegment.SwitchAttacks((int)Attack);
                }

                if (phase == 1 && Attack == (int)Attacks.Minefield)
                {
                    NPC.Center = arenaCenter - Vector2.UnitX * MINEFIELD_ARENA_DISTANCE;
                }

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
            NPC.netUpdate = true;

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
        }

        const int DASHING_COOLDOWN = 90;
        const int DASHING_TIME = 60;
        void DashingStarSpam()
        {
            switch (AttackTimer)
            {
                case > DASHING_TIME:
                    targetPosition = playerDirection.SafeNormalize(Vector2.Zero);
                    NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 10;
                    break;
                case > 0 and < DASHING_TIME:
                    NPC.velocity = targetPosition.SafeNormalize(Vector2.Zero) * 40;
                    break;
                case 0:
                    AttackTimer = DASHING_COOLDOWN;
                    return;
            }

            AttackTimer--;
        }

        const int CIRCLING_START_TIME = 480;
        const int CIRCLING_POS_TIME = 420;
        void Circling()
        {
            switch (AttackTimer)
            {
                case > CIRCLING_POS_TIME:
                    AttackCount = CIRCLING_ARENA_DISTANCE;
                    targetPosition = arenaCenter + (Vector2.UnitY * CIRCLING_ARENA_DISTANCE);
                    NPC.velocity = NPC.Center.DirectionTo(targetPosition) * (NPC.Center.Distance(targetPosition) / 12);
                    break;
                case > 0:
                    targetPosition = (-Vector2.UnitY * AttackCount);
                    NPC.velocity = NPC.Center.DirectionTo(arenaCenter + targetPosition);
                    NPC.Center = arenaCenter + targetPosition.RotatedBy(MathHelper.ToRadians(-AttackTimer * 3));
                    AttackCount -= 3;
                    break;
                case 0:
                    AttackTimer = CIRCLING_START_TIME;
                    return;
            }

            AttackTimer--;
        }

        const int CHASING_START_TIME = 240;
        const int CHASING_ATTACK_COOLDOWN = 10;
        const int CHASING_MAX_PROJ_COUNT = 12;
        void Chasing()
        {
            switch (AttackTimer)
            {
                case > 0:
                    NPC.velocity = playerDirection.SafeNormalize(Vector2.Zero) * 6;
                    break;
                case 0:
                    if (AttackCount < CHASING_MAX_PROJ_COUNT)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            AttackTimer2 = (int)MathHelper.ToRadians(Main.rand.Next(-15, 15));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(AttackTimer2) * 20, Proj["Starshot"], NPC.damage, 1);
                        }
                        AttackTimer = CHASING_ATTACK_COOLDOWN;
                        AttackCount++;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        AttackTimer = CHASING_START_TIME;
                        AttackCount = 0;
                    }
                    return;
            }
            AttackTimer--;
        }

        const int MINEFIELD_TIME = 720;
        const int MINEFIELD_ATTACK_RATE = 30;
        void Minefield()
        {
            switch (AttackTimer)
            {
                case > 0:
                    if (AttackTimer % MINEFIELD_ATTACK_RATE == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(i * (MathHelper.PiOver4 / 2)) * 10, Proj["Starshot"], NPC.damage, 1);
                            }
                        }
                    }
                    float yPos = (float)Math.Sin(MathHelper.ToRadians((AttackCount + MINEFIELD_ARENA_DISTANCE) / 4)) * (MINEFIELD_ARENA_DISTANCE / 4); // Sine to move up and down while going right
                    Vector2 pos = arenaCenter + new Vector2(AttackCount, yPos);
                    AttackCount += 8;
                    NPC.velocity = NPC.Center.DirectionTo(pos) * NPC.Center.Distance(pos) / 12;
                    break;
                case 0:
                    AttackTimer = MINEFIELD_TIME;
                    AttackCount = -MINEFIELD_ARENA_DISTANCE;
                    return;
            }
            AttackTimer--;
        }


        void MoveToPos(Vector2 pos, float xAccel = 1f, float yAccel = 1f, float xSpeed = 1f, float ySpeed = 1f)
        {
            Vector2 direction = NPC.Center.DirectionTo(pos);
            float XaccelMod = Math.Sign(direction.X) - Math.Sign(NPC.velocity.X);
            float YaccelMod = Math.Sign(direction.Y) - Math.Sign(NPC.velocity.Y);
            NPC.velocity += new Vector2(XaccelMod * xAccel + xSpeed * Math.Sign(direction.X), YaccelMod * yAccel + ySpeed * Math.Sign(direction.Y));
        }

        const int BASE_ARENA_DISTANCE = 3000;
        const int CIRCLING_ARENA_DISTANCE = 2000;
        const int CHASING_ARENA_DISTANCE = 1500;
        public const int MINEFIELD_ARENA_DISTANCE = 2500;
        public void Arena()
        {
            float targetArenaDistance = BASE_ARENA_DISTANCE;
            arenaFollow = true;

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.DashingStarSpam:
                        arenaFollow = false;
                        break;
                    case (int)Attacks.Circling:
                        arenaFollow = false;
                        targetArenaDistance = CIRCLING_ARENA_DISTANCE;
                        break;
                    case (int)Attacks.Chasing:
                        arenaFollow = false;
                        targetArenaDistance = CIRCLING_ARENA_DISTANCE;
                        break;
                    case (int)Attacks.Minefield:
                        arenaFollow = false;
                        targetArenaDistance = MINEFIELD_ARENA_DISTANCE;
                        break;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Spheres.Count < 40)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 120f);

                        Spheres.Add(sphere);
                    }
                    NPC.netUpdate = true;
                }
            }

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
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedNebulaMaster, -1);
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