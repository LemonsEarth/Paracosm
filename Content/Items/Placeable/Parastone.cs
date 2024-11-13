using Paracosm.Content.Items.Materials;
using Paracosm.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Placeable
{
    public class Parastone : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ParastoneBlock>());
            Item.width = 16;
            Item.height = 16;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100);
            recipe.AddIngredient(ItemID.StoneBlock, 100);
            recipe.AddIngredient(ModContent.ItemType<Parashard>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ModContent.ItemType<ParastoneWallItem>());
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();
        }
    }
}
