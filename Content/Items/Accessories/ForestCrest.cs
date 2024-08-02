using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;

namespace Paracosm.Content.Items.Accessories
{
    public class ForestCrest : ModItem
    {
        static readonly int sentryBoost = 1;
        static readonly float moveSpeedBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(sentryBoost, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.maxTurrets += sentryBoost;
            player.moveSpeed += moveSpeedBoost / 100;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(ItemID.Sunflower, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
