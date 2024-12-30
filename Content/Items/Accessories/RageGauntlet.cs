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
    public class RageGauntlet : ModItem
    {
        static readonly float damageBoost = 25;
        static readonly float critBoost = 30;
        static readonly float moveSpeedBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 44;
            Item.accessory = true;
            Item.defense = 12;
            Item.value = Item.sellPrice(0, 8);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.panic = true;
            if (player.statLife < player.statLifeMax2 / 5f)
            {
                player.GetDamage(DamageClass.Generic) += damageBoost / 100f;
                player.GetCritChance(DamageClass.Generic) += critBoost;
                player.moveSpeed += moveSpeedBoost / 100f;
                player.aggro += 600;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PowerGlove);
            recipe.AddIngredient(ItemID.PanicNecklace);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
