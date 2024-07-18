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
            player.GetModPlayer<WindWarriorBreastplatePlayer>().WindWarriorBreastplate = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SoulofFlight, 15);
            recipe1.AddIngredient(ItemID.Feather, 4);
            recipe1.AddIngredient(ItemID.TitaniumBar, 12);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.SoulofFlight, 15);
            recipe2.AddIngredient(ItemID.Feather, 4);
            recipe2.AddIngredient(ItemID.AdamantiteBar, 12);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }

    public class WindWarriorBreastplatePlayer : ModPlayer
    {
        public bool WindWarriorBreastplate = false;

        public override void ResetEffects()
        {
            WindWarriorBreastplate = false;
        }

        public override void PostUpdateEquips()
        {
            if (WindWarriorBreastplate == false)
            {
                return;
            }

            Player.wingTimeMax += Player.wingTimeMax / 2;
        }
    }
}
