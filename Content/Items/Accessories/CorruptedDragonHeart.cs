using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class CorruptedDragonHeart : ModItem
    {
        const int maxLifeBoost = 10;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(maxLifeBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statLifeMax2 += player.statLifeMax2 / maxLifeBoost;
            player.GetModPlayer<ParacosmPlayer>().corruptedDragonHeart = true;
        }
    }
}
