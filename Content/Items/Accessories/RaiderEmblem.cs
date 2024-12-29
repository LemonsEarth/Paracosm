using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class RaiderEmblem : ModItem
    {
        static readonly float damageBoost = 20;
        static readonly float critBoost = 25;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ParacosmRarity.LightGreen;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += damageBoost / 100f;
            player.GetCritChance(DamageClass.Ranged) += critBoost;
            player.aggro -= 900;
        }
    }
}
