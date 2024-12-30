using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class WarlockEmblem : ModItem
    {
        static readonly float damageBoost = 35;
        static readonly int manaBoost = 60;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, manaBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ParacosmRarity.PinkPurple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += damageBoost / 100f;
            player.statManaMax2 += manaBoost;
            player.aggro -= 100;
        }
    }
}
