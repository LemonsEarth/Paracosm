using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class CharmOfLife : ModItem
    {
        static readonly float lifeRegenBoost = 6;
        static readonly float maxLifeBoost = 40;
        int timer = 0;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(maxLifeBoost, lifeRegenBoost);

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            return incomingItem.type != ItemID.CharmofMyths;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ItemRarityID.Green;
            Item.lifeRegen = (int)lifeRegenBoost;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
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
            recipe.AddIngredient(ItemID.CharmofMyths);
            recipe.AddIngredient(ItemID.LifeFruit, 5);
            recipe.AddIngredient(ItemID.Ectoplasm, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
