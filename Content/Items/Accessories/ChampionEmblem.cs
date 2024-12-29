using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class ChampionEmblem : ModItem
    {
        static readonly float damageBoost = 20;
        static readonly float critBoost = 8;
        static readonly float meleeSpeedBoost = 20;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, meleeSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 30);
            Item.rare = ParacosmRarity.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += damageBoost / 100f;
            player.GetCritChance(DamageClass.Melee) += critBoost;
            player.GetAttackSpeed(DamageClass.Melee) += meleeSpeedBoost / 100f;
        }
    }
}
