using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Celestial
{
    [AutoloadEquip(EquipType.Legs)]
    public class StardustDragonTail : ModItem
    {
        static readonly float moveSpeedBoost = 8;
        static readonly float damageBoost = 15;
        static readonly float whipRangeBoost = 33;
        static readonly int minionBoost = 2;
        static readonly int sentryBoost = 2;

        static readonly int setMinionBoost = 1;
        static readonly int setSentryBoost = 1;
        static readonly float set2DamageBoost = 20;
        static readonly float set2CritBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(moveSpeedBoost, damageBoost, whipRangeBoost, minionBoost, sentryBoost);
        public static LocalizedText setBonusText;
        public static LocalizedText setBonusText2;
        bool setbonus2 = false;

        public override void SetStaticDefaults()
        {
            setBonusText = this.GetLocalization("SetBonus").WithFormatArgs(setMinionBoost, setSentryBoost);
            setBonusText2 = this.GetLocalization("SetBonus2").WithFormatArgs(set2DamageBoost, set2CritBoost);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.defense = 2;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
            Item.value = Item.sellPrice(0, 15, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {     
            player.moveSpeed += moveSpeedBoost / 100;
            player.GetDamage(DamageClass.Summon) += damageBoost / 100f;
            player.whipRangeMultiplier += whipRangeBoost / 100f;
            player.maxMinions += minionBoost;
            player.maxTurrets += sentryBoost;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            if (head.type == ModContent.ItemType<ChampionsCrown>() && body.type == ModContent.ItemType<VortexControlUnit>())
            {
                setbonus2 = true;
                return true;
            }
            setbonus2 = false;
            return (head.type == ItemID.StardustHelmet && body.type == ItemID.StardustBreastplate);
        }

        public override void UpdateArmorSet(Player player)
        {
            if (setbonus2)
            {
                player.setBonus = setBonusText2.Value;
                player.GetDamage(DamageClass.Generic) += set2DamageBoost / 100f;
                player.GetCritChance(DamageClass.Generic) += set2CritBoost;
            }
            else
            {
                player.setBonus = setBonusText.Value;
                player.maxMinions += 1;
                player.maxTurrets += 1;
            }
            player.GetModPlayer<ParacosmPlayer>().stardustTailSet = true;
        }
    }
}
