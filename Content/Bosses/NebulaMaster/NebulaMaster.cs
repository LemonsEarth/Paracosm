using Microsoft.Xna.Framework;
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
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.VortexMothership
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
                int maxVal = phase == 2 ? 2 : 3;
                if (value > maxVal || value < 0)
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
        int[] attackDurations = { 300, 180, 900, 1260 };
        int[] attackDurations2 = { 900, 900, 1200};
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()}
        };
        public enum Attacks
        {

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
            NPC.damage = (int)(NPC.damage * balance);
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

            if (!Terraria.Graphics.Effects.Filters.Scene["ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
            {
                Terraria.Graphics.Effects.Filters.Scene.Activate("ScreenTintShader").GetShader().UseColor(new Color(225, 56, 255));
            }

            foreach (var p in Main.player)
            {
                p.nebulaMonolithShader = true;
            }

            Arena();

            if (AITimer < 60)
            {
                NPC.dontTakeDamage = true;
                NPC.velocity = new Vector2(0, 2);
                NPC.Opacity += 1f / 60f;
                AITimer++;
                Attack = 0;
                attackDuration = attackDurations[(int)Attack];
                Terraria.Graphics.Effects.Filters.Scene["ScreenTintShader"].GetShader().UseProgress(AITimer / 60);
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.life <= (NPC.lifeMax * 0.6f) && !phase2FirstTime)
            {
                phase2FirstTime = true;
                phase = 2;
                SwitchAttacks();
                NPC.netUpdate = true;
            }

            NPC.velocity = Vector2.Zero;

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            attackDuration--;
            AITimer++;
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
            }
            NPC.netUpdate = true;
        }
        const float BaseArenaDistance = 1500;
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

                Spheres[i].velocity = (pos - Spheres[i].position).SafeNormalize(Vector2.Zero) * (Spheres[i].position.Distance(pos) / 20);
                Spheres[i].timeLeft = 180;


            }
            if (arenaDistance < BaseArenaDistance)
            {
                arenaDistance += BaseArenaDistance / 60f;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > 120)
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
            Terraria.Graphics.Effects.Filters.Scene.Deactivate("ScreenTintShader");
            return true;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
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
    }
}