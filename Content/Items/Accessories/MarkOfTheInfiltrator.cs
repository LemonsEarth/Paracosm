using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class MarkOfTheInfiltrator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 40);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().infiltratorMark = true;
            player.blackBelt = true;
            player.empressBrooch = true;
            player.wingTimeMax = 9999;
            player.wingTime = player.wingTimeMax;
            player.frogLegJumpBoost = true;
            player.accFlipper = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MasterNinjaGear);
            recipe.AddIngredient(ItemID.FrogFlipper);
            recipe.AddIngredient(ItemID.EmpressFlightBooster);
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 30);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
