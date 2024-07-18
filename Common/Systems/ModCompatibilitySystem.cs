using Paracosm.Content.Bosses;
using Paracosm.Content.Items.Consumables;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Paracosm.Common.Systems
{
    public class ModCompatibilitySystem : ModSystem
    {
        public override void PostSetupContent()
        {
            BossChecklistCompatibility();
        }

        private void BossChecklistCompatibility()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod BossChecklist))
            {
                return;
            }

            if (BossChecklist.Version < new Version(2, 2))
            {
                return;
            }

            //Log Divine Seeker
            string internalName = "DivineSeeker";
            float progression = 11.9f; //Plantera - 12f
            Func<bool> downed = () => DownedBossSystem.downedDivineSeeker;
            int bossType = ModContent.NPCType<DivineSeeker>();

            int spawnItemType = ModContent.ItemType<CosmicDust>();
            LocalizedText spawnInfo = Language.GetText("Mods.Paracosm.NPCs.DivineSeeker.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additional = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemType,
                ["spawnInfo"] = spawnInfo
            };

            BossChecklist.Call("LogBoss", Mod, internalName, progression, downed, bossType, additional);
        }
    }
}
