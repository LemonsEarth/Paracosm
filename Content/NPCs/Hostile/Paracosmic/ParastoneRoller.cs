using Microsoft.Xna.Framework;
using Paracosm.Content.Biomes.Overworld;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Placeable;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Paracosmic
{
    public class ParastoneRoller : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        bool positionChosen = false;
        Vector2 RandomPos
        {
            get => new Vector2(NPC.ai[1], NPC.ai[2]);
            set
            {
                NPC.ai[1] = value.X;
                NPC.ai[2] = value.Y;
            }
        }

        ref float DashTimer => ref NPC.ai[3];


        public override void SetDefaults()
        {
            NPC.width = 84;
            NPC.height = 84;
            NPC.lifeMax = 80;
            NPC.defense = 3;
            NPC.damage = 20;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.value = 300;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<ParacosmicDistortion>().Type };
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ParacosmicDistortion>().ModBiomeBestiaryInfoElement),
                });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Parashard>(), minimumDropped: 2, maximumDropped: 4));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Parastone>(), minimumDropped: 12, maximumDropped: 24));
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255)
            {
                NPC.TargetClosest(false);
            }
            Player player = Main.player[NPC.target];
            AITimer++;
            NPC.rotation = MathHelper.ToRadians(-AITimer / 2 * (NPC.velocity.Length() + 3));

            if (positionChosen == false)
            {
                ChoosePosition(player);
            }
            else
            {
                DashTimer++;
                if (DashTimer < 30)
                {
                    NPC.velocity = (RandomPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.Center.Distance(RandomPos) / 10;
                }
                if (DashTimer == 30)
                {
                    RandomPos = player.MountedCenter;
                }
                if (DashTimer == 60)
                {
                    Dash();
                }
                if (DashTimer >= 75 && DashTimer < 90)
                {
                    NPC.velocity = Vector2.Zero;
                }
                if (DashTimer == 90)
                {
                    positionChosen = false;
                    DashTimer = 0;
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            DashTimer -= 2;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            DashTimer -= 2;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome<ParacosmicDistortion>() ? 0.05f : 0f;
        }

        void ChoosePosition(Player target)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                RandomPos = target.Center + new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(50, 100), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(50, 100));
                positionChosen = true;
            }
            NPC.netUpdate = true;
        }

        void Dash()
        {
            NPC.velocity = (RandomPos - NPC.Center).SafeNormalize(Vector2.Zero) * 30;
        }
    }
}
