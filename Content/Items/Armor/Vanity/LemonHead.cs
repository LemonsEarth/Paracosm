﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class LemonHead : ModItem
    {

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 28;
            Item.defense = 7;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Lemon, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
