using Paracosm.Content.Biomes;
using Paracosm.Content.NPCs.Hostile;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool paracosmicBurn = false;
        public bool solarBurn = false;

        public override void ResetEffects(NPC npc)
        {
            paracosmicBurn = false;
            solarBurn = false;
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
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<ParacosmicDistortion>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<Wanderer>(), 0.3f);
                pool.Add(ModContent.NPCType<ParastoneRoller>(), 0.1f);
            }
        }
    }
}
