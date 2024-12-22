using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class CorruptedLifeCrystal : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 26;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.Purple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().corruptedLifeCrystal = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.LifeCrystal);
            recipe1.AddIngredient(ItemID.ShadowScale, 8);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.LifeCrystal);
            recipe2.AddIngredient(ItemID.TissueSample, 8);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();
        }
    }
}
