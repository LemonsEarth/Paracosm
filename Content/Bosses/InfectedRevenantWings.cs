using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Hostile;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System.IO;
using Terraria.Audio;

namespace Paracosm.Content.Bosses
{
    public class InfectedRevenantWings : ModNPC
    {
        InfectedRevenantBody body;
        bool noAnim = false;
        public int ParentIndex
        {
            get => (int)NPC.ai[0];
            set
            {
                NPC.ai[0] = value;
            }
        }

        ref float AITimer => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers drawMods = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawMods);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 540;
            NPC.height = 304;
            NPC.lifeMax = 100000;
            NPC.dontTakeDamage = true;
            NPC.damage = 0;
            NPC.defense = 30;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCHit1;
            NPC.value = 0;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1;
            NPC.noGravity = true;
            NPC.npcSlots = 1f;
            NPC.SpawnWithHigherTime(2);
            NPC.hide = true;
            NPC.netAlways = true;
        }

        public override bool CheckActive()
        {
            return false;
        }

        public static int BodyType()
        {
            return ModContent.NPCType<InfectedRevenantBody>();
        }

        public override void AI()
        {
            NPC bodyNPC = Main.npc[ParentIndex];
            if (Main.netMode != NetmodeID.MultiplayerClient && (Main.npc[(int)ParentIndex] == null || !Main.npc[(int)ParentIndex].active || Main.npc[(int)ParentIndex].type != BodyType()))
            {
                NPC.active = false;
                NPC.life = 0;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                return;
            }
            InfectedRevenantBody body = (InfectedRevenantBody)bodyNPC.ModNPC;
            this.body = body;
            NPC.alpha = bodyNPC.alpha;
            NPC.position = body.WingsPos;
            if (AITimer % 45 == 0 && body.phaseTransition == false)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { MaxInstances = 1, PitchVariance = 1.0f, Volume = 0.5f });
            }
            AITimer++;
            if (body.phaseTransition)
            {
                noAnim = true;
            }
            else
            {
                noAnim = false;
            }
        }


        public override void FindFrame(int frameHeight)
        {
            int frameDurLong = 12;
            int frameDurShort = 6;
            int frameDur = 0;
            if (noAnim)
            {
                NPC.frame.Y = 0;
                return;
            }

            if (NPC.frame.Y == 1 * frameHeight || NPC.frame.Y == 3 * frameHeight)
            {
                frameDur = frameDurLong;
            }
            else
            {
                frameDur = frameDurShort;
            }
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

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsMoonMoon.Add(index);
        }
    }
}
