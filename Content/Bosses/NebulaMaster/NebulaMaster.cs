using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.NebulaMaster
{
    [AutoloadBossHead]
    public class NebulaMaster : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        public float Attack
        {
            get { return NPC.ai[1]; }
            private set
            {
                if (value > 1 || value < 0)
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

        float attackDuration = 0;
        int[] attackDurations = { 480, 450, 900, 1260 };
        int[] attackDurations2 = { 900, 900, 1200 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;
        bool spawnedAura = false;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()},
            {"SpeedFlames",  ModContent.ProjectileType<SpeedyNebulousFlames>()},
            {"Aura", ModContent.ProjectileType<NebulousAuraHostile>()}
        };

        public enum Attacks
        {
            SpeedingFlames,
            RapidDashes
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
            Main.npcFrameCount[NPC.type] = 9;
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.NebulaPillar,
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 86;
            NPC.height = 154;
            NPC.Opacity = 1;
            NPC.lifeMax = 600000;
            NPC.defense = 40;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCHit52;
            NPC.value = 5000000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SunBornCyclone");
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.6f);
            NPC.damage = (int)(NPC.damage * balance * 0.5f);
            NPC.defense = 40;
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
            writer.Write(spawnedAura);
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
            spawnedAura = reader.ReadBoolean();
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
                NPC.Opacity = 0;
            }

            Lighting.AddLight(NPC.Center, 100, 100, 100);

            if (!Terraria.Graphics.Effects.Filters.Scene["ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("ScreenTintShader").GetShader().UseColor(new Color(255, 230, 255));
            }

            NPC.spriteDirection = -Math.Sign(playerDirection.X);

            foreach (var p in Main.player)
            {
                p.nebulaMonolithShader = true;
            }

            if (AITimer > INTRO_DURATION - 60)
            {
                Arena();
            }

            if (AITimer < INTRO_DURATION)
            {
                Intro();
                AITimer++;
                return;
            }
            NPC.dontTakeDamage = false;
            float rotmul = NPC.spriteDirection == 1 ? MathHelper.Pi : 0;
            NPC.rotation = playerDirection.ToRotation() + rotmul;

            if (NPC.life <= (NPC.lifeMax * 0.6f) && !phase2FirstTime)
            {
                phase2FirstTime = true;
                phase = 2;
                SwitchAttacks();
                NPC.netUpdate = true;
            }

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            if (phase == 1)
            {
                switch (Attack)
                {
                    case (int)Attacks.SpeedingFlames:
                        SpeedingFlames();
                        break;
                    case (int)Attacks.RapidDashes:
                        RapidDashes();
                        break;
                }
            }

            attackDuration--;
            AITimer++;
        }

        const int INTRO_DURATION = 180;
        void Intro()
        {
            if (AITimer == INTRO_DURATION - 90)
            {
                AdvancedPopupRequest message = new AdvancedPopupRequest();
                message.Color = Color.White;
                message.DurationInFrames = 60;
                message.Velocity = 5 * -Vector2.UnitY;
                int rand = Main.rand.Next(0, 5);
                string msgKey = "Intro" + rand;
                message.Text = Language.GetTextValue($"Mods.Paracosm.NPCs.NebulaMaster.Dialogue.{msgKey}");

                PopupText.NewText(message, NPC.Center + -Vector2.UnitY * NPC.height / 2);
            }
            NPC.dontTakeDamage = true;
            NPC.velocity = NPC.Center.DirectionTo(player.Center + new Vector2(500, 0)) * (NPC.Center.Distance(player.Center + new Vector2(500, 0)) / 12);
            NPC.Opacity += 1f / 20f;
            Attack = 0;
            attackDuration = attackDurations[(int)Attack];
            Terraria.Graphics.Effects.Filters.Scene["ScreenTintShader"].GetShader().UseProgress(AITimer / 60);
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
                spawnedAura = false;
                if (Attack == (int)Attacks.RapidDashes)
                {
                    AttackTimer = DASH_START_COOLDOWN;
                }
                foreach (var proj in Proj)
                {
                    if (proj.Key != "Sphere")
                        DeleteProjectiles(proj.Value);
                }
            }
            NPC.netUpdate = true;
        }

        const int RANDOM_POS_TIME = 60;
        const int MOVE_TIME = 30;
        const int ATTACK_RATE = 15;
        void SpeedingFlames()
        {
            switch (AttackTimer)
            {
                case RANDOM_POS_TIME:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        targetPosition = NPC.Center + new Vector2(Main.rand.NextFloat(-300, 300), Main.rand.NextFloat(-300, 300));
                        NPC.netUpdate = true;
                    }
                    break;
                case > MOVE_TIME:
                    MoveToPos(targetPosition, 1, 1, 2f, 2f);
                    break;
                case > 0:
                    if (AttackTimer % ATTACK_RATE == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Vector2 direction = playerDirection.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(i * 15));
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction * 2, Proj["SpeedFlames"], NPC.damage / 2, 1, ai0: 30, ai1: 25, ai2: 1);
                            }
                        }
                    }
                    break;
                case 0:
                    AttackTimer = RANDOM_POS_TIME + 1;
                    break;
            }

            AttackTimer--;
        }

        const int DASH_START_COOLDOWN = 120;
        const int DASH_COOLDOWN = 35;
        const int DASH_SPEED = 40;
        void RapidDashes()
        {
            switch (AttackTimer)
            {
                case > DASH_COOLDOWN and < DASH_START_COOLDOWN:
                    if (!spawnedAura)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Proj["Aura"], NPC.damage, 1, ai1: NPC.whoAmI);
                        spawnedAura = true;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GemAmethyst);
                        dust.noGravity = true;
                        dust.velocity = new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));
                    }
                    break;
                case DASH_COOLDOWN:
                    targetPosition = playerDirection;
                    break;
                case > 0 and < DASH_COOLDOWN:
                    NPC.velocity = targetPosition.SafeNormalize(Vector2.Zero) * DASH_SPEED;
                    break;
                case <= 0:
                    AttackTimer = DASH_COOLDOWN + 1;
                    break;
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

        const float BaseArenaDistance = 3500;
        public void Arena()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Spheres.Count < 40)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        var sphere = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)), Vector2.Zero, Proj["Sphere"], NPC.damage, 1, ai1: 60f);

                        Spheres.Add(sphere);
                    }
                    NPC.netUpdate = true;
                }
            }

            for (int i = 0; i < Spheres.Count; i++)
            {
                Vector2 pos = NPC.Center + new Vector2(0, -arenaDistance).RotatedBy(i * MathHelper.ToRadians(9)).RotatedBy(MathHelper.ToRadians(AITimer));
                if (Spheres[i].type != Proj["Sphere"])
                {
                    continue;
                }
                Spheres[i].velocity = (pos - Spheres[i].position).SafeNormalize(Vector2.Zero) * (Spheres[i].position.Distance(pos) / 50);
                Spheres[i].timeLeft = 180;
            }
            if (arenaDistance < BaseArenaDistance)
            {
                arenaDistance += BaseArenaDistance / (INTRO_DURATION / 3);
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > INTRO_DURATION + 30)
                {
                    player.AddBuff(ModContent.BuffType<Infected>(), 2);
                }

                if (NPC.Center.Distance(player.MountedCenter) < arenaDistance + 50 && AITimer > INTRO_DURATION - 60)
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

        public override bool CheckDead()
        {
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("ScreenTintShader");
            return true;
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
            classicRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<VortexianPlating>(), 1, 4, 8));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.FragmentVortex, 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            classicRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<HorizonSplitter>(), ModContent.ItemType<TheCrucible>()));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<VortexMothershipBossBag>()));
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
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedVortexMothership, -1);
            for (int i = 0; i < 16; i++)
            {
                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(0, NPC.width), Main.rand.Next(0, NPC.height)), new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(2f, 5f));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle drawRect = texture.Frame(1, Main.npcFrameCount[Type], 0, 0);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            for (int k = NPC.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin;
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRect, color, NPC.rotation, drawOrigin, NPC.scale, spriteEffects, 0);
            }
            return true;
        }
    }
}