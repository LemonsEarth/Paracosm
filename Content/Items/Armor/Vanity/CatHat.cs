﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class CatHat : ModItem
    {

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.defense = 1;
            Item.rare = ItemRarityID.Gray;
            Item.value = Item.sellPrice(0, 0, 50, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.05f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FlinxFur, 3);
            recipe.AddIngredient(ItemID.RichMahogany, 69);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
