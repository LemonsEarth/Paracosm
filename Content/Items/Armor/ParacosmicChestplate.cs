using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;

namespace Paracosm.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class ParacosmicChestplate : ModItem
    {
        static readonly float damageBoost = 5;
        static readonly float critBoost = 8;
        static readonly float drBoost = 7;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, drBoost);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.defense = 20;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 8, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.endurance += drBoost / 100;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 15);
            recipe.AddIngredient(ItemID.HallowedBar, 16);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
