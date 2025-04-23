using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class ItemRecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.HermesBoots)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CloudinaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.BlizzardinaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.SandstorminaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.FrogLeg)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.Aglet)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.AnkletoftheWind)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ShoeSpikes)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ClimbingClaws)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.FlyingCarpet)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.LavaCharm)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.LuckyHorseshoe)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ObsidianRose)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.NaturesGift)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CreativeWings)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.BandofRegeneration)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.MagmaStone)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.PhilosophersStone)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CrossNecklace)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.StarCloak)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();

            Recipe.Create(ItemID.MagicQuiver)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.TitanGlove)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.MoonCharm)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.PutridScent)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.FleshKnuckles)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.FrozenTurtleShell)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void PostAddRecipes()
        {
            foreach (Recipe recipe in Main.recipe)
            {
                if (recipe.createItem.type == ItemID.Zenith)
                {
                    recipe.AddIngredient(ModContent.ItemType<SolarCore>(), 5);
                    recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 5);
                    recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 5);
                    recipe.AddIngredient(ModContent.ItemType<PureStardust>(), 5);
                }
            }
        }
    }
}
