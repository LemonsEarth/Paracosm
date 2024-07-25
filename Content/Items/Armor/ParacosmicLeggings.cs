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
    [AutoloadEquip(EquipType.Legs)]
    public class ParacosmicLeggings : ModItem
    {
        static readonly float moveSpeedBoost = 12;
        static readonly float critBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(critBoost, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.defense = 16;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 6, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.moveSpeed += moveSpeedBoost / 100;
            player.jumpSpeedBoost += 1;
            player.GetModPlayer<ParacosmicLeggingsPlayer>().ParacosmicLeggings = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 10);
            recipe.AddIngredient(ModContent.ItemType<CosmicFlames>(), 8);
            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    public class ParacosmicLeggingsPlayer : ModPlayer
    {
        public bool ParacosmicLeggings = false;

        public override void ResetEffects()
        {
            ParacosmicLeggings = false;
        }

        public override void PostUpdateEquips()
        {
            if (ParacosmicLeggings == false)
            {
                return;
            }
        }
    }
}
