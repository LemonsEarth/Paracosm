using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class CommanderEmblem : ModItem
    {
        static readonly float damageBoost = 15;
        static readonly int minionBoost = 2;
        static readonly int sentryBoost = 2;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, minionBoost, sentryBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ParacosmRarity.LightBlue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += damageBoost / 100f;
            player.maxMinions += minionBoost;
            player.maxTurrets += sentryBoost;
        }
    }
}
