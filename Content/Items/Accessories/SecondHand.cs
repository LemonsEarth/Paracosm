using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class SecondHand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 32;
            Item.accessory = true;
            Item.value = 5000;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().secondHand = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 6);
            recipe.AddIngredient(ItemID.Bone, 25);
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }
}
