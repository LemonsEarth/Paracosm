using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class UniversalCoating : ModItem
    {
        public static readonly float METEOR_VELOCITY = 14f;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().starfallCoating = true;
            player.GetModPlayer<ParacosmPlayer>().craterCoating = true;
            player.GetModPlayer<ParacosmPlayer>().spiritCoating = true;
            player.GetModPlayer<ParacosmPlayer>().universalCoating = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<StarfallCoating>());
            recipe.AddIngredient(ModContent.ItemType<CraterCoating>());
            recipe.AddIngredient(ModContent.ItemType<SpiritCoating>());
            recipe.AddIngredient(ItemID.BeetleHusk, 8);
            recipe.AddIngredient(ItemID.MeteoriteBar, 20);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
