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
    public class SuperManaStar : ModItem
    {
        static readonly int manaBoost = 60;
        static readonly int manaCostReduction = 50;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(manaBoost, manaCostReduction);

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            return incomingItem.type != ItemID.MagnetFlower;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 16);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statManaMax2 += manaBoost;
            player.manaCost -= manaCostReduction / 100f;
            player.manaMagnet = true;
            player.manaFlower = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MagnetFlower);
            recipe.AddIngredient(ModContent.ItemType<SolarCore>(), 6);
            recipe.AddIngredient(ModContent.ItemType<PureStardust>(), 8);
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 30);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
