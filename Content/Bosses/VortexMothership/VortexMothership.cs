using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.VortexMothership
{
    [AutoloadBossHead]
    public class VortexMothership : ModNPC
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
                int maxVal = phase == 1 ? 3 : 2;
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

        bool spawnedWeapons = false;
        bool spawnedUFOs = false;

        float attackDuration = 0;
        int[] attackDurations = { 300, 180, 900, 1260 };
        int[] attackDurations2 = { 900, 900, 1200 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()}
        };

        Dictionary<string, int> Summonables = new Dictionary<string, int>
        {
            {"Tesla", ModContent.NPCType<VortexTeslaGun>()},
            {"UFO", ModContent.NPCType<VortexUFO>()},
            {"StormDiver", NPCID.VortexRifleman },
            {"HornetQueen", NPCID.VortexHornetQueen }
        };

        public Vector2[] gunOffsets { get; private set; } =
        {
            new Vector2(-60, 90),
            new Vector2(60, 90),
            new Vector2(-400, -60),
            new Vector2(400, -60),
        };
        VortexTeslaGun[] teslaGuns = new VortexTeslaGun[4];

        public enum Attacks
        {
            TeslashotSpam,
            CenterBlast,
            PredictiveShots,
            Mix
        }

        public enum Attacks2
        {
            ChillTeslaShots,
            MineSpam,
            Lasers,
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
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitScale = 0.2f,
                PortraitPositionYOverride = -150
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.VortexPillar,
                new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 1124;
            NPC.height = 368;
            NPC.Opacity = 1;
            NPC.lifeMax = 750000;
            NPC.defense = 100;
            NPC.damage = 40;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCHit57;
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

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (phase == 1)
            {
                modifiers.FinalDamage *= 0.8f;
            }
        }

        // Reduce damage taken from piercing projectiles
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.penetrate == -1 || projectile.penetrate > 4)
            {
                modifiers.FinalDamage /= 3;
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.6f);
            NPC.damage = (int)(NPC.damage * balance);
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
                Terraria.Graphics.Effects.Filters.Scene.Activate("ScreenTintShader").GetShader().UseColor(new Color(89, 255, 225));
            }

            foreach (var p in Main.player)
            {
                p.vortexMonolithShader = true;
            }

            Arena();
            SpawnWeapons();

            if (AITimer < INTRO_DURATION)
            {
                Intro();
                AITimer++;
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.life <= (NPC.lifeMax * 0.4f) && !phase2FirstTime)
            {
                phase2FirstTime = true;
                phase = 2;
                SwitchAttacks();
                NPC.netUpdate = true;
            }

            if (phase == 2)
            {
                NPC.defDefense = 120;
                int spawnSpeedDiv = (NPC.life < (NPC.lifeMax / 4)) ? 2 : 1;
                if (AttackTimer % (900 / spawnSpeedDiv) == 0)
                {
                    SpawnEnemies();
                }

                if (AttackTimer % (3000 / spawnSpeedDiv) == 0)
                {
                    SpawnUFOs();
                    spawnedUFOs = false;
                }
                if (AITimer % 60 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(0, NPC.width), Main.rand.Next(0, NPC.height)), new Vector2(Main.rand.NextFloat(-3, 3)), Main.rand.Next(61, 64), Main.rand.NextFloat(1f, 5f));
                    }
                }

                AttackTimer++;
            }

            NPC.velocity = Vector2.Zero;

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            attackDuration--;
            AITimer++;
        }

        const int INTRO_DURATION = 60;
        void Intro()
        {
            NPC.dontTakeDamage = true;
            NPC.velocity = new Vector2(0, 2);
            NPC.Opacity += 1f / 60f;
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
                foreach (VortexTeslaGun gun in teslaGuns)
                {
                    gun.SwitchAttacks((int)Attack);
                }

                AttackCount = 0;
                AttackCount2 = 0;
                AttackTimer = 0;
                AttackTimer2 = 0;
            }
            NPC.netUpdate = true;

            if (Spheres.Any(p => p.active == false))
            {
                Spheres.Clear();
            }
        }

        void SpawnWeapons()
        {
            if (spawnedWeapons)
            {
                return;
            }

            spawnedWeapons = true;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                NPC teslaGunNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + gunOffsets[i], Summonables["Tesla"], NPC.whoAmI, NPC.whoAmI, i);
                teslaGuns[i] = (VortexTeslaGun)teslaGunNPC.ModNPC;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: teslaGunNPC.whoAmI);
                }
            }
        }

        void SpawnUFOs()
        {
            if (spawnedUFOs)
            {
                return;
            }

            spawnedUFOs = true;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            int countNPCs = Main.npc.Count(npc => npc.active && npc.type == Summonables["UFO"]);
            int count = NPC.life < (NPC.lifeMax / 4) ? 5 - countNPCs : 3 - countNPCs;
            for (int i = 0; i < count; i++)
            {
                NPC ufoNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, Summonables["UFO"], NPC.whoAmI, NPC.whoAmI);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: ufoNPC.whoAmI);
                }
            }
        }

        void SpawnEnemies()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            int count = NPC.life < (NPC.lifeMax / 4) ? 4 : 2;
            for (int i = 0; i < Main.rand.Next(count, count + 1); i++)
            {
                NPC diver = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(Main.rand.Next(-100, 100)), Summonables["StormDiver"], NPC.whoAmI);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: diver.whoAmI);
                }
            }

            for (int i = 0; i < Main.rand.Next(1, 2); i++)
            {
                NPC queen = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(Main.rand.Next(-100, 100)), Summonables["HornetQueen"], NPC.whoAmI);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: queen.whoAmI);
                }
            }
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
                arenaDistance += BaseArenaDistance / INTRO_DURATION;
            }

            foreach (var player in Main.ActivePlayers)
            {
                if (NPC.Center.Distance(player.MountedCenter) > arenaDistance + 50 && AITimer > INTRO_DURATION * 2)
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