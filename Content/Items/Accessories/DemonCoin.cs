using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class DemonCoin : ModItem
    {
        static float damageBoost = 0;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(0, 1);
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 999;
        }
    }
}
