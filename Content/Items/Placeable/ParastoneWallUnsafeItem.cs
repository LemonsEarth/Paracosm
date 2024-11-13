using Paracosm.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Placeable
{
    public class ParastoneWallUnsafeItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ParastoneWallItem>();
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<ParastoneWallUnsafe>());
            Item.width = 32;
            Item.height = 32;
        }
    }
}
