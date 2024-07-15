using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using System;

namespace Paracosm.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class WindWarriorHeadgear : ModItem
    {
        static readonly int maxMinionBoost = 1;
        static readonly float damageBoost = 5;

        static readonly float setBonusDamage = 15;
        static readonly float setBonusCrit = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, maxMinionBoost);
        public static LocalizedText setBonusText;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
            setBonusText = this.GetLocalization("SetBonus").WithFormatArgs(setBonusDamage, setBonusCrit);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.defense = 3;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.maxMinions += maxMinionBoost;
            player.GetModPlayer<WindWarriorHeadgearPlayer>().WindWarriorHeadgear = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<WindWarriorBreastplate>() && legs.type == ModContent.ItemType<WindWarriorLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = setBonusText.Value;
            player.GetDamage(DamageClass.Generic) += (15 + MathHelper.Clamp(Math.Abs(player.velocity.Y), 0, 20)) / 100;
        }
    }

    public class WindWarriorHeadgearPlayer : ModPlayer
    {
        public bool WindWarriorHeadgear = false;

        public override void ResetEffects()
        {
            WindWarriorHeadgear = false;
        }

        public override void PostUpdateEquips()
        {
            if (WindWarriorHeadgear == false)
            {
                return;
            }
        }
    }
}
