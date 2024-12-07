using Paracosm.Content.Biomes.Overworld;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Bosses.InfectedRevenant;
using Paracosm.Content.Bosses.NebulaMaster;
using Paracosm.Content.Bosses.VortexMothership;
using Paracosm.Content.NPCs.Hostile.Paracosmic;
using Paracosm.Content.NPCs.Hostile.Void;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool paracosmicBurn = false;
        public bool solarBurn = false;
        public bool melting = false;

        public override void ResetEffects(NPC npc)
        {
            paracosmicBurn = false;
            solarBurn = false;
            melting = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (paracosmicBurn)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= 150;
                if (damage < 150)
                {
                    damage = 150;
                }
            }

            if (solarBurn)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= 300;
                if (damage < 300)
                {
                    damage = 300;
                }
            }

            if (melting)
            {
                npc.defense = 0;
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.SolarFlare);
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<ParacosmicDistortion>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<Wanderer>(), 0.3f);
                pool.Add(ModContent.NPCType<ParastoneRoller>(), 0.1f);
            }

            if (spawnInfo.Player.InModBiome<VoidMid>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<ShadowSeeker>(), 0.05f);
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (npc.type == ModContent.NPCType<VortexMothership>() || npc.type == ModContent.NPCType<VortexTeslaGun>() || npc.type == ModContent.NPCType<InfectedRevenantCorruptHead>() || npc.type == ModContent.NPCType<InfectedRevenantCrimsonHead>() || npc.type == ModContent.NPCType<NebulaMaster>())
            {
                return false;
            }
            return true;
        }
    }
}
