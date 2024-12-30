using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class VoidHeart : ModItem
    {
        int timer = 0;
        static readonly float maxLifeBoost = 80;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(maxLifeBoost);

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            return incomingItem.type != ModContent.ItemType<CharmOfLife>() && incomingItem.type != ModContent.ItemType<CorruptedLifeCrystal>();
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ParacosmRarity.DarkGray;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().voidHeart = true;
            player.statLifeMax2 += (int)maxLifeBoost;
            if (player.HasBuff(BuffID.PotionSickness) && timer == 0)
            {
                var potionSickness = player.FindBuffIndex(BuffID.PotionSickness);
                player.buffTime[potionSickness] -= player.buffTime[potionSickness] / 4;
                timer = 45 * 60;
            }

            if (!player.HasBuff(BuffID.PotionSickness))
            {
                timer = 0;
            }

            if (timer > 0) timer--;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<CorruptedLifeCrystal>());
            recipe.AddIngredient(ModContent.ItemType<CharmOfLife>());
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 35);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
