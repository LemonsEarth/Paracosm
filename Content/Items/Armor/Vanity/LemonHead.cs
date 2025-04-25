using Terraria;
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
            Item.defense = 1;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.UseSound = SoundID.Item2;
        }

        public override bool? UseItem(Player player)
        {
            player.AddBuff(BuffID.WellFed, 7200);
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Lemon, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
