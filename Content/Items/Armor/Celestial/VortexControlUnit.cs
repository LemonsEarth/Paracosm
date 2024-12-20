using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Celestial
{
    [AutoloadEquip(EquipType.Body)]
    public class VortexControlUnit : ModItem
    {
        static readonly float damageBoost = 20;
        static readonly float critBoost = 20;
        static readonly float moveSpeedBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, moveSpeedBoost);
        public static LocalizedText setBonusText;

        public override void SetStaticDefaults()
        {
            setBonusText = this.GetLocalization("SetBonus");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.defense = 8;
            Item.expert = true;
            Item.rare = ItemRarityID.Expert;
            Item.value = Item.sellPrice(0, 15, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += damageBoost / 100;
            player.GetCritChance(DamageClass.Ranged) += critBoost;
            player.moveSpeed += moveSpeedBoost / 100f;
            player.aggro -= 200;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return (head.type == ItemID.VortexHelmet && legs.type == ItemID.VortexLeggings) ||
                (head.type == ModContent.ItemType<ChampionsCrown>() && legs.type == ModContent.ItemType<StardustDragonTail>());
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = setBonusText.Value;
            player.GetModPlayer<ParacosmPlayer>().vortexControlUnitSet = true;
        }
    }
}
