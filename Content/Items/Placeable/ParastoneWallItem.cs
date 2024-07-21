using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Paracosm.Content.Walls;

namespace Paracosm.Content.Items.Placeable
{
    public class ParastoneWallItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ParastoneWallUnsafeItem>();
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<ParastoneWall>());
            Item.width = 32;
            Item.height = 32;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(4);
            recipe.AddIngredient(ModContent.ItemType<Parastone>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
