using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class ParashardSigil : ModItem
    {
        static readonly float damageBoost = 8;
        static readonly float critBoost = 8;
        static readonly float defenseBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, defenseBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 40;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Expert;
            Item.defense = (int)defenseBoost;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.GetModPlayer<ParacosmPlayer>().parashardSigil = true;
        }
    }
}
