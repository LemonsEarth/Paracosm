using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Bosses.DivineSeeker;
using Paracosm.Content.Bosses.InfectedRevenant;
using Paracosm.Content.Bosses.SolarChampion;
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
            string internalName = "DivineSeeker";
            float progression = 11.9f; //Plantera - 12f
            Func<bool> downed = () => DownedBossSystem.downedDivineSeeker;
            int bossType = ModContent.NPCType<DivineSeeker>();

            int spawnItemType = ModContent.ItemType<CosmicDust>();
            LocalizedText spawnInfo = Language.GetText("Mods.Paracosm.NPCs.DivineSeeker.BossChecklistCompatibility.SpawnInfo");
            List<int> collectibles = new List<int>()
            {
                ModContent.ItemType<Content.Items.Placeable.Furniture.DivineSeekerRelic>()
            };

            Dictionary<string, object> additional = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemType,
                ["spawnInfo"] = spawnInfo,
                ["collectibles"] = collectibles
            };

            BossChecklist.Call("LogBoss", Mod, internalName, progression, downed, bossType, additional);


            //Log Infected Revenant
            string internalName1 = "InfectedRevenantBody";
            float progression1 = 16.1f; //Betsy - 16f
            Func<bool> downed1 = () => DownedBossSystem.downedInfectedRevenant;
            int bossType1 = ModContent.NPCType<InfectedRevenantBody>();

            int spawnItemType1 = ModContent.ItemType<AncientCallingHorn>();
            LocalizedText spawnInfo1 = Language.GetText("Mods.Paracosm.NPCs.InfectedRevenantBody.BossChecklistCompatibility.SpawnInfo");

            List<int> collectibles1 = new List<int>()
            {
                ModContent.ItemType<Content.Items.Placeable.Furniture.InfectedRevenantRelic>()
            };
            Action<SpriteBatch, Rectangle, Color> portrait = (SpriteBatch spriteBatch, Rectangle rect, Color color) =>
            {
                Texture2D texture = ModContent.Request<Texture2D>("Paracosm/Content/Textures/BossChecklist/InfectedRevenantBossChecklistPortrait").Value;
                Vector2 centered = new Vector2((int)(rect.X + (rect.Width / 2) - (texture.Width / 2)), (int)(rect.Y + (rect.Height / 2) - (texture.Height / 2)));
                spriteBatch.Draw(texture, centered, color);
            };

            Dictionary<string, object> additional1 = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemType1,
                ["spawnInfo"] = spawnInfo1,
                ["collectibles"] = collectibles1,
                ["customPortrait"] = portrait
            };

            BossChecklist.Call("LogBoss", Mod, internalName1, progression1, downed1, bossType1, additional1);

            //Log SolarChampion
            string internalName2 = "SolarChampion";
            float progression2 = 18.1f; //Moon Lord - 18f
            Func<bool> downed2 = () => DownedBossSystem.downedSolarChampion;
            int bossType2 = ModContent.NPCType<SolarChampion>();

            int spawnItemType2 = ModContent.ItemType<ChallengersSeal>();
            LocalizedText spawnInfo2 = Language.GetText("Mods.Paracosm.NPCs.SolarChampion.BossChecklistCompatibility.SpawnInfo");

            Dictionary<string, object> additional2 = new Dictionary<string, object>()
            {
                ["spawnItems"] = spawnItemType2,
                ["spawnInfo"] = spawnInfo2
            };

            BossChecklist.Call("LogBoss", Mod, internalName2, progression2, downed2, bossType2, additional2);
        }


    }
}
