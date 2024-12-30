using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class OathOfVengeance : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 58;
            Item.accessory = true;
            Item.value = 15000;
            Item.rare = ParacosmRarity.DarkGray;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().secondHand = true;
            player.GetModPlayer<ParacosmPlayer>().starfallCoating = true;
            player.GetModPlayer<ParacosmPlayer>().craterCoating = true;
            player.GetModPlayer<ParacosmPlayer>().spiritCoating = true;
            player.GetModPlayer<ParacosmPlayer>().universalCoating = true;
            player.GetModPlayer<ParacosmPlayer>().oathOfVengeance = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<SecondHand>());
            recipe.AddIngredient(ModContent.ItemType<UniversalCoating>());
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 40);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
