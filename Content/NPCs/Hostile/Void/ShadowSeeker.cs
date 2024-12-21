using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Void
{
    public class ShadowSeeker : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        ref float DoDeath => ref NPC.ai[1];
        ref float DeadTimer => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
        }

        public override void SetDefaults()
        {
            NPC.width = 64;
            NPC.height = 64;
            NPC.lifeMax = 2000;
            NPC.defense = 10;
            NPC.damage = 50;
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.value = 3000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<VoidMid>().Type };
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1f;
            NPC.noGravity = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<VoidMid>().ModBiomeBestiaryInfoElement),
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
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    DoDeath = Main.rand.Next(0, 2);
                    NPC.netUpdate = true;
                }
            }

            NPC.velocity /= 1.05f;
            if (NPC.life > 1 && DeadTimer == 0)
            {
                NPC.rotation = NPC.Center.DirectionTo(player.Center).ToRotation();

                if (AITimer % 120 == 0)
                {
                    NPC.velocity = NPC.Center.DirectionTo(player.Center) * 50;
                    NPC.netUpdate = true;
                }
            }
            else if (DoDeath > 0)
            {
                NPC.dontTakeDamage = true;
                NPC.frame.Y = 4 * 64;
                NPC.rotation = MathHelper.ToRadians(DeadTimer * 4);
                if (DeadTimer > 120)
                {
                    SpawnRandomEnemies();
                    DoDeath = 0;
                    NPC.life = 0;
                }
                DeadTimer++;
            }

            Lighting.AddLight(NPC.Center, 100, 100, 100);
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Granite, newColor: Color.Black, Scale: Main.rand.NextFloat(1.2f, 2.4f));
            dust.noGravity = true;
            AITimer++;
        }

        void SpawnRandomEnemies()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(Main.rand.NextFloat(-32, 32)), Main.rand.NextFromList(405, 407, 418, 421, 427), NPC.whoAmI);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
                }
            }
        }

        public override bool CheckDead()
        {
            if (DoDeath > 0)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.frame.Y = 4 * 64;
                return false;
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCDeath11);
                return true;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome<VoidMid>() ? 0.05f : 0;
        }

        public override void FindFrame(int frameHeight)
        {
            if (DeadTimer > 0) return;
            int frameDur = 6;
            NPC.frameCounter += 1;
            if (NPC.frameCounter > frameDur)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
                if (NPC.frame.Y > 3 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidEssence>(), minimumDropped: 2, maximumDropped: 6));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle drawRect = texture.Frame(1, Main.npcFrameCount[Type], 0, 0);

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            for (int k = NPC.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRect, color, NPC.rotation, drawOrigin, 1.2f, SpriteEffects.None, 0);
            }
            return true;
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
