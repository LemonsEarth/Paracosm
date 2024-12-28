using Microsoft.Build.Framework;
using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class StarfallCoating : ModItem
    {
        public static readonly float STAR_VELOCITY = 10f;
        public static readonly int STAR_DAMAGE_CAP = 10;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(STAR_DAMAGE_CAP);

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().starfallCoating = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FallenStar, 10);
            recipe.AddIngredient(ItemID.Feather, 7);
            recipe.AddIngredient(ItemID.Silk, 6);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }
}
