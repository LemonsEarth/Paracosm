using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class WindWarriorBreastplate : ModItem
    {
        static readonly float manaCostReduction = 20;
        static readonly float damageBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, manaCostReduction);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.defense = 10;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 7, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.manaCost -= manaCostReduction / 100;
            player.GetModPlayer<ParacosmPlayer>().windWarriorBreastplate = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SoulofFlight, 15);
            recipe1.AddIngredient(ItemID.Feather, 4);
            recipe1.AddIngredient(ItemID.CobaltBar, 12);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.SoulofFlight, 15);
            recipe2.AddIngredient(ItemID.Feather, 4);
            recipe2.AddIngredient(ItemID.PalladiumBar, 12);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}
