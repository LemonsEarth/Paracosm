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
    public class AshenForestCrest : ModItem
    {
        static readonly int sentryBoost = 2;
        static readonly float summonDamageBoost = 7;
        static readonly float moveSpeedBoost = 14;
        int timer = 0;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(sentryBoost, summonDamageBoost, moveSpeedBoost);

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
            player.maxTurrets += sentryBoost;
            player.GetDamage(DamageClass.Summon) += summonDamageBoost / 100;
            player.moveSpeed += moveSpeedBoost / 100;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ForestCrest>(), 1);
            recipe.AddIngredient(ItemID.AshWood, 50);
            recipe.AddIngredient(ItemID.HellstoneBar, 4);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
}
