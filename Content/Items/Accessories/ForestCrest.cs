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
        static readonly int minionBoost = 1;
        static readonly float moveSpeedBoost = 10;
        int timer = 0;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(minionBoost, moveSpeedBoost);

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
            player.maxMinions += minionBoost;
            player.moveSpeed += moveSpeedBoost / 100;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(ItemID.Shadewood, 20);
            recipe.AddIngredient(ItemID.BorealWood, 20);
            recipe.AddIngredient(ItemID.PalmWood, 20);
            recipe.AddIngredient(ItemID.RichMahogany, 20);
            recipe.AddIngredient(ItemID.Aglet);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();

            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.Wood, 20);
            recipe1.AddIngredient(ItemID.Ebonwood, 20);
            recipe1.AddIngredient(ItemID.BorealWood, 20);
            recipe1.AddIngredient(ItemID.PalmWood, 20);
            recipe1.AddIngredient(ItemID.RichMahogany, 20);
            recipe1.AddIngredient(ItemID.Aglet);
            recipe1.AddTile(TileID.WorkBenches);
            recipe1.Register();
        }
    }
}
