using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Bosses.DivineSeeker;
using Paracosm.Content.Bosses.InfectedRevenant;
using Paracosm.Content.Bosses.NebulaMaster;
using Paracosm.Content.Bosses.SolarChampion;
using Paracosm.Content.Bosses.StardustLeviathan;
using Paracosm.Content.Bosses.VortexMothership;
using Paracosm.Content.Items.Consumables;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

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
            int spawnItemTypeDS = ModContent.ItemType<CosmicDust>();
            LocalizedText spawnInfoDS = Language.GetText("Mods.Paracosm.NPCs.DivineSeeker.BossChecklistCompatibility.SpawnInfo");
            List<int> collectiblesDS = new List<int>()
            {
                ModContent.ItemType<Content.Items.Placeable.Furniture.DivineSeekerRelic>()
            };

            Dictionary<string, object> additionalDS = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeDS,
                ["spawnInfo"] = spawnInfoDS,
                ["collectibles"] = collectiblesDS
            };

            LogBoss(
                BossChecklist,
                "DivineSeeker",
                11.9f,
                () => DownedBossSystem.downedDivineSeeker,
                ModContent.NPCType<DivineSeeker>(),
                additionalDS
                );




            //Log Infected Revenant
            int spawnItemTypeIR = ModContent.ItemType<AncientCallingHorn>();
            LocalizedText spawnInfoIR = Language.GetText("Mods.Paracosm.NPCs.InfectedRevenantBody.BossChecklistCompatibility.SpawnInfo");
            List<int> collectiblesIR = new List<int>()
            {
                ModContent.ItemType<Content.Items.Placeable.Furniture.InfectedRevenantRelic>()
            };
            Action<SpriteBatch, Rectangle, Color> portraitIR = (SpriteBatch spriteBatch, Rectangle rect, Color color) =>
            {
                Texture2D texture = ModContent.Request<Texture2D>("Paracosm/Assets/Textures/BossChecklist/InfectedRevenantBossChecklistPortrait").Value;
                Vector2 centered = new Vector2((int)(rect.X + (rect.Width / 2) - (texture.Width / 2)), (int)(rect.Y + (rect.Height / 2) - (texture.Height / 2)));
                spriteBatch.Draw(texture, centered, color);
            };
            Dictionary<string, object> additionalIR = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeIR,
                ["spawnInfo"] = spawnInfoIR,
                ["collectibles"] = collectiblesIR,
                ["customPortrait"] = portraitIR
            };

            LogBoss(
                BossChecklist,
                "InfectedRevenantBody",
                16.1f,
                () => DownedBossSystem.downedInfectedRevenant,
                ModContent.NPCType<InfectedRevenantBody>(),
                additionalIR
                );




            //Log Solar Champion
            int spawnItemTypeSC = ModContent.ItemType<ChallengersSeal>();
            LocalizedText spawnInfoSC = Language.GetText("Mods.Paracosm.NPCs.SolarChampion.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additionalSC = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeSC,
                ["spawnInfo"] = spawnInfoSC
            };

            LogBoss(
                BossChecklist,
                "SolarChampion",
                18.1f,
                () => DownedBossSystem.downedSolarChampion,
                ModContent.NPCType<SolarChampion>(),
                additionalSC
                );




            //Log Vortex Mothership
            int spawnItemTypeVM = ModContent.ItemType<StarSpanCommunicator>();
            LocalizedText spawnInfoVM = Language.GetText("Mods.Paracosm.NPCs.VortexMothership.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additionalVM = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeVM,
                ["spawnInfo"] = spawnInfoVM
            };

            LogBoss(
                BossChecklist,
                "VortexMothership",
                18.2f,
                () => DownedBossSystem.downedVortexMothership,
                ModContent.NPCType<VortexMothership>(),
                additionalVM
                );




            //Log Nebula Master
            int spawnItemTypeNM = ModContent.ItemType<ArcaneTablet>();
            LocalizedText spawnInfoNM = Language.GetText("Mods.Paracosm.NPCs.NebulaMaster.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additionalNM = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeNM,
                ["spawnInfo"] = spawnInfoNM
            };

            LogBoss(
                BossChecklist,
                "NebulaMaster",
                18.3f,
                () => DownedBossSystem.downedNebulaMaster,
                ModContent.NPCType<NebulaMaster>(),
                additionalNM
                );




            //Log Stardust Leviathan
            int spawnItemTypeSL = ModContent.ItemType<DragonFood>();
            LocalizedText spawnInfoSL = Language.GetText("Mods.Paracosm.NPCs.StardustLeviathanHead.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additionalSL = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemTypeSL,
                ["spawnInfo"] = spawnInfoSL
            };

            LogBoss(
                BossChecklist,
                "StardustLeviathanHead",
                18.4f,
                () => DownedBossSystem.downedStardustLeviathan,
                ModContent.NPCType<StardustLeviathanHead>(),
                additionalSL
                );
        }

        void LogBoss(Mod BossChecklist, string internalName, float progression, Func<bool> downed, int bossType, Dictionary<string, object> additional)
        {
            BossChecklist.Call("LogBoss", Mod, internalName, progression, downed, bossType, additional);
        }

    }
}
