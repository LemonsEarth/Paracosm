using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class CommandersWill : ModItem
    {
        static readonly int sentryBoost = 4;
        static readonly float summonDamageBoost = 12;
        static readonly float nonSummonDamageDecrease = 20;
        static readonly float moveSpeedBoost = 16;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(sentryBoost, summonDamageBoost, nonSummonDamageDecrease, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 35);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().commandersWill = true;
            player.maxTurrets += sentryBoost;
            player.GetDamage(DamageClass.Summon) += summonDamageBoost / 100;
            player.GetDamage(DamageClass.Melee) -= nonSummonDamageDecrease / 100;
            player.GetDamage(DamageClass.Ranged) -= nonSummonDamageDecrease / 100;
            player.GetDamage(DamageClass.Magic) -= nonSummonDamageDecrease / 100;
            player.moveSpeed += moveSpeedBoost / 100;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<AshenForestCrest>(), 1);
            recipe.AddIngredient(ItemID.HuntressBuckler, 1);
            recipe.AddIngredient(ItemID.ApprenticeScarf, 1);
            recipe.AddIngredient(ItemID.MonkBelt, 1);
            recipe.AddIngredient(ItemID.SquireShield, 1);
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 12);
            recipe.AddIngredient(ModContent.ItemType<CosmicFlames>(), 8);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 10);
            recipe.AddIngredient(ItemID.Ectoplasm, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
