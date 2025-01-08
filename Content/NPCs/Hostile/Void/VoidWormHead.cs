using Microsoft.Xna.Framework;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Materials;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Void
{
    public class VoidWormHead : ModNPC
    {
        int BodyType => ModContent.NPCType<VoidWormBody>();
        int TailType => ModContent.NPCType<VoidWormTail>();
        const int MAX_SEGMENT_COUNT = 30;
        int SegmentCount = 0;

        ref float AITimer => ref NPC.ai[0];

        List<VoidWormBody> Segments = new List<VoidWormBody>();

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
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome<VoidLow>() ? 0.005f : 0;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 104;
            NPC.height = 104;
            NPC.Opacity = 1;
            NPC.lifeMax = 250000;
            NPC.defense = 80;
            NPC.damage = 120;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.DD2_BetsyDeath;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<VoidLow>().Type };
            NPC.value = 50000;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0;
            NPC.noGravity = true;
            NPC.npcSlots = 2;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }
            Player player = Main.player[NPC.target];

            NPC.velocity = NPC.Center.DirectionTo(player.Center) * 10;
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
            if (AITimer == 0)
            {
                SpawnSegments();
                PlayRoar(1.5f);
            }
            NPC.Opacity = (float)Math.Clamp(Math.Sin(MathHelper.ToRadians(AITimer * 3)), 0f, 1f);
            AITimer++;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        void SpawnSegments()
        {
            int latestNPC = NPC.whoAmI;
            while (SegmentCount < MAX_SEGMENT_COUNT - 2) // Body segments, excluding head and tail
            {
                latestNPC = SpawnSegment(BodyType, latestNPC);
                VoidWormBody bodySegment = (VoidWormBody)Main.npc[latestNPC].ModNPC;
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


        void PlayRoar(float volume = 1f)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = volume, PitchRange = (-0.4f, 0.8f) }, NPC.Center);
            SoundEngine.PlaySound(SoundID.Roar with { Volume = volume, PitchRange = (-1f, -0.8f) }, NPC.Center);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidEssence>(), minimumDropped: 2, maximumDropped: 6));
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<VoidCharm>(), 10, 5));
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
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