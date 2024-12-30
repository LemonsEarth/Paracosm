using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class EnergyCore : ModItem
    {
        static readonly float damageBoost = 22;
        static readonly float DRBoost = 22;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, DRBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.defense = 6;
            Item.value = Item.sellPrice(0, 16);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.statLife <= player.statLifeMax2 / 2f)
            {
                player.endurance += DRBoost / 100f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += damageBoost / 100f;
            }
            player.noKnockback = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<RoundShield>());
            recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 6);
            recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 4);
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 13);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
