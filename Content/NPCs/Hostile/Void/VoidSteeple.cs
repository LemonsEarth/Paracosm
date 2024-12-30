using Microsoft.Xna.Framework;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Void
{
    public class VoidSteeple : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        ref float AttackTimer => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 4000;
            NPC.defense = 10;
            NPC.damage = 30;
            NPC.HitSound = SoundID.NPCHit13;
            NPC.DeathSound = SoundID.NPCDeath12;
            NPC.value = 6000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 5;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<VoidMid>().Type };
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1f;
            NPC.noGravity = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                });
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255)
            {
                NPC.TargetClosest(false);
            }
            Player player = Main.player[NPC.target];

            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
            }

            NPC.rotation = NPC.Center.DirectionTo(player.Center).ToRotation() + MathHelper.Pi;

            switch (AttackTimer)
            {
                case >= 60:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center) * 7;
                    break;
                case > 30 and < 60:
                    NPC.velocity /= 1.05f;
                    break;
                case <= 30 and > 0:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center);
                    if (AttackTimer % 10 == 0)
                    {
                        SpawnLeeches();
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.Center.DirectionTo(player.Center) * 5, ModContent.ProjectileType<BloodBlast>(), NPC.damage, 1f);
                        }
                    }
                    break;
                case 0:
                    AttackTimer = 300;
                    return;
            }
            AttackTimer--;

            Lighting.AddLight(NPC.Center, 10, 10, 10);
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Granite, newColor: Color.Black, Scale: Main.rand.NextFloat(1.2f, 2.4f));
            dust.noGravity = true;
            AITimer++;
        }

        void SpawnLeeches()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(Main.rand.NextFloat(-32, 32)), 117, NPC.whoAmI);
            npc.damage = 50;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome<VoidMid>() ? 0.03f : 0;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameDur = 6;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 1 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidEssence>(), minimumDropped: 3, maximumDropped: 7));
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
