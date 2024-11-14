using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.WindWarrior
{
    [AutoloadEquip(EquipType.Legs)]
    public class WindWarriorLeggings : ModItem
    {
        static readonly float moveSpeedBoost = 75;
        static readonly float critBoost = 8;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(critBoost, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.defense = 6;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.moveSpeed += moveSpeedBoost / 100;
            player.jumpSpeedBoost += 4;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SoulofFlight, 12);
            recipe1.AddIngredient(ItemID.Feather, 3);
            recipe1.AddIngredient(ItemID.CobaltBar, 7);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.SoulofFlight, 12);
            recipe2.AddIngredient(ItemID.Feather, 3);
            recipe2.AddIngredient(ItemID.PalladiumBar, 7);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}
