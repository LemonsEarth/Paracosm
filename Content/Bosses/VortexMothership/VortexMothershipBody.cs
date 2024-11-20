using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Systems;
using Paracosm.Content.Bosses.InfectedRevenant;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.BossBags;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Items.Weapons.Ranged;
using Paracosm.Content.Items.Weapons.Summon;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Paracosm.Content.Bosses.VortexMothership
{
    [AutoloadBossHead]
    public class VortexMothershipBody : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        public float Attack
        {
            get { return NPC.ai[1]; }
            private set
            {
                NPC.ai[1] = value;
            }
        }
        ref float AttackTimer => ref NPC.ai[2];
        ref float AttackCount => ref NPC.ai[3];

        int AttackTimer2 = 0;
        int AttackCount2 = 0;

        public int damage = 20;

        bool phase2FirstTime = false;
        int phase = 1;

        bool spawnedWeapons = false;

        float attackDuration = 0;
        int[] attackDurations = { 300, 180, 1200, 600 };
        int[] attackDurations2 = { 1200, 900, 1200, 600 };
        public Player player { get; private set; }
        public Vector2 playerDirection { get; private set; }
        Vector2 targetPosition = Vector2.Zero;
        float arenaDistance = 0;

        List<Projectile> Spheres = new List<Projectile>();
        Dictionary<string, int> Proj = new Dictionary<string, int>
        {
            {"Sphere", ModContent.ProjectileType<BorderSphere>()}
        };

        Dictionary<string, int> Weapons = new Dictionary<string, int>
        {
            {"Tesla", ModContent.NPCType<VortexTeslaGun>()}
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
            CenterBlast
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
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.SolarPillar,
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                //new FlavorTextBestiaryInfoElement(this.GetLocalizedValue("Bestiary")),
            });
        }

        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.width = 1124;
            NPC.height = 368;
            NPC.Opacity = 1;
            NPC.lifeMax = 500000;
            NPC.defense = 100;
            NPC.damage = 0;
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
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SunBornCyclone");
            }
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

            if (AITimer < 60)
            {
                NPC.dontTakeDamage = true;
                NPC.velocity = new Vector2(0, 2);
                NPC.Opacity += 1f / 60f;
                AITimer++;
                Attack = -1;
                Terraria.Graphics.Effects.Filters.Scene["ScreenTintShader"].GetShader().UseProgress(AITimer / 60);
                return;
            }
            NPC.dontTakeDamage = false;

            if (NPC.life <= NPC.lifeMax / 2 && !phase2FirstTime)
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
                if (Attack > 1)
                {
                    Attack = 0;
                }
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
                NPC teslaGunNPC = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + gunOffsets[i], Weapons["Tesla"], NPC.whoAmI, NPC.whoAmI, i);
                teslaGuns[i] = (VortexTeslaGun)teslaGunNPC.ModNPC;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: teslaGunNPC.whoAmI);
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

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)Math.Ceiling(NPC.lifeMax * balance * 0.65f);
            damage = (int)(damage * balance);
            NPC.defense = 50;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            /*int frameDur = 8;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }*/
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            //target.AddBuff(ModContent.BuffType<SolarBurn>(), 180);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule classicRule = new LeadingConditionRule(new Conditions.NotExpert());
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.FragmentSolar, 1, 10, 20));
            classicRule.OnSuccess(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            classicRule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<HorizonSplitter>(), ModContent.ItemType<TheCrucible>()));
            npcLoot.Add(classicRule);
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SolarChampionBossBag>()));
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
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedSolarChampion, -1);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            /*Texture2D texture = TextureAssets.Npc[Type].Value;
            //Rectangle drawRect = texture.Frame(1, Main.npcFrameCount[Type], 0, 0);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            }*/
            return true;
        }
    }
}