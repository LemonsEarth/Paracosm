using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Paracosm.Common.Utils;
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
using Paracosm.Content.Projectiles.Hostile;
using System.Linq;


namespace Paracosm.Content.Bosses
{
    [AutoloadBossHead]
    public class SolarChampion : ModNPC
    {
        public ref float AITimer => ref NPC.ai[0];
        public ref float Attack => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float AttackCount => ref NPC.ai[3];

        public int AttackTimer2 = 0;
        public int AttackCount2 = 0;

        public float attackDuration = 0;
        int[] attackDurations = { 20000, 1800, 360, 420, 600 };

        int SwordType => ModContent.ProjectileType<SolarBlade>();

        enum Attacks
        {
            DashingSword
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            /* NPCID.Sets.NPCBestiaryDrawModifiers drawMod = new NPCID.Sets.NPCBestiaryDrawModifiers()
             {
                 PortraitPositionYOverride = -15f,
                 PortraitScale = 0.8f
             };
             NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawMod);*/
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
            NPC.width = 100;
            NPC.height = 100;
            NPC.Opacity = 0;
            NPC.lifeMax = 200000;
            NPC.defense = 100;
            NPC.damage = 100;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 100000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 10;
            NPC.SpawnWithHigherTime(30);

            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SeveredSpace");
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];
            Vector2 playerDirection = NPC.Center.DirectionTo(player.Center);

            if (player.dead || NPC.Center.Distance(player.MountedCenter) > 2500)
            {
                NPC.EncourageDespawn(300);
            }

            if (AITimer < 60)
            {
                NPC.dontTakeDamage = true;
                NPC.velocity = new Vector2(0, -2);
                NPC.Opacity += (1f / 60f);
                AITimer++;
                Attack = -1;
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.velocity.Length() > 10)
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, playerDirection.X * MathHelper.ToRadians(30), MathHelper.ToRadians(1));
            }
            else
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, MathHelper.ToRadians(1));
            }

            if (attackDuration <= 0)
            {
                SwitchAttacks();
            }

            switch (Attack)
            {
                case 0:
                    DashingSword(player);
                    break;
                case 1:
                    DashingSword(player);
                    break;
                case 2:
                    DashingSword(player);
                    break;
                case 3:
                    DashingSword(player);
                    break;
            }
            attackDuration--;
            AITimer++;
        }

        void SwitchAttacks()
        {
            Attack++;
            if (Attack > 3)
            {
                Attack = 0;
            }
            attackDuration = attackDurations[(int)Attack];
            AttackCount = 0;
            AttackTimer = 0;
            AttackTimer2 = 30;
        }

        Vector2 tempPos = Vector2.Zero;
        Vector2 leftOffset = new Vector2(-500, 0);
        Vector2 targetPosition = Vector2.Zero;
        float swordOffset = 200;
        float rotSpeedMul = 1;
        bool isDashing = false;
        List<Projectile> Swords = new List<Projectile>();

        public void DashingSword(Player player)
        {
            if (AttackCount < 4)
            {
                if (AttackTimer == 0)
                {
                    rotSpeedMul = 1;
                    targetPosition = player.Center + new Vector2(Main.rand.Next(350, 500) * -Math.Sign(player.Center.X - NPC.Center.X), Main.rand.NextFloat(-100, 100));
                }
                if (Swords.Count < 4 || Swords.Any(proj => proj.active == false))
                {
                    SummonSword();
                }

                for (int i = 0; i < Swords.Count; i++)
                {
                    var sword = Swords[i];
                    Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer * rotSpeedMul)).RotatedBy(i * MathHelper.PiOver2);
                    sword.Center = NPC.Center + rotatedPos;
                    sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    sword.timeLeft = 180;
                }

                if (AttackTimer > 60 && AttackTimer < 180)
                {
                    rotSpeedMul += 0.02f;
                    isDashing = true;
                    NPC.velocity = -NPC.Center.DirectionTo(player.Center).SafeNormalize(Vector2.Zero) * 2;
                }
                if (AttackTimer == 120)
                {
                    tempPos = player.Center;
                }
                if (AttackTimer == 180 && isDashing)
                {
                    NPC.velocity = NPC.Center.DirectionTo(tempPos).SafeNormalize(Vector2.Zero) * 50;
                    rotSpeedMul = 8;
                }

                if (!isDashing)
                {
                    NPC.velocity = (targetPosition - NPC.Center) / 20;
                }

                if (AttackTimer >= 240 && isDashing)
                {
                    isDashing = false;
                    AttackTimer = 0;
                    AttackCount++;
                }
            }
            else
            {
                NPC.velocity = NPC.Center.DirectionTo(player.Center) * 5;
                if (AttackTimer >= 30)
                {
                    AttackTimer = 0;
                }
                if (AttackTimer == 0)
                {
                    foreach (var sword in Swords)
                    {
                        sword.velocity *= 20;
                    }
                    AttackCount2++;
                    Swords.Clear();
                }
                if (AttackTimer > 15)
                {
                    if (Swords.Count < 4)
                    {
                        SummonSword();
                        for (int i = 0; i < Swords.Count; i++)
                        {
                            var sword = Swords[i];
                            Vector2 rotatedPos = new Vector2(0, swordOffset).RotatedBy(MathHelper.ToRadians(AttackTimer * rotSpeedMul)).RotatedBy(MathHelper.ToRadians(i * (90 + AttackCount2 * 15)));
                            sword.Center = NPC.Center + rotatedPos;
                            sword.velocity = (sword.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        }
                    }
                }
            }
                AttackTimer++;
        }

        void SummonSword(int count = 1)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Swords.Add(Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
                    NPC.Center + new Vector2(0, NPC.height / 2).RotatedBy(MathHelper.ToRadians(i * (360 / count))),
                    Vector2.Zero,
                    SwordType,
                    NPC.damage,
                    1));
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.65f);
            NPC.damage = (int)(NPC.damage * balance);
            NPC.defense = 30;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
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
    }
}