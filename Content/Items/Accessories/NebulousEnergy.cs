using Paracosm.Common.Players;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class NebulousEnergy : ModItem
    {
        static readonly float damageBoost = 5;
        static readonly float critBoost = 5;
        static readonly float defenseBoost = 5;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost);

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Expert;
            Item.defense = (int)defenseBoost;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += damageBoost / 100;
            player.GetCritChance(DamageClass.Magic) += critBoost;
            player.AddBuff(ModContent.BuffType<NebulousPower>(), 2);
            player.GetModPlayer<ParacosmPlayer>().nebulousEnergy = true;
        }
    }
}
