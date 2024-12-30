using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class SpiritCoating : ModItem
    {
        public static readonly float SPIRIT_VELOCITY = 20f;
        public static readonly int SPIRIT_DAMAGE_CAP = 120;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(SPIRIT_DAMAGE_CAP);

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 5);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().spiritCoating = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Ectoplasm, 6);
            recipe.AddIngredient(ItemID.SpookyWood, 50);
            recipe.AddIngredient(ItemID.Silk, 6);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }
}
