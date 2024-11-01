using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Paracosm.Content.Buffs;
using Microsoft.Xna.Framework.Graphics;

namespace Paracosm.Content.Items.Armor
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
            Item.defense = 7;
            Item.rare = ItemRarityID.Gray;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FlinxFur, 1);
            recipe.AddIngredient(ItemID.RichMahogany, 69);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
