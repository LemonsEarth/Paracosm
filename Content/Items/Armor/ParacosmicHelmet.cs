using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Paracosm.Content.Buffs;

namespace Paracosm.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class ParacosmicHelmet : ModItem
    {
        static readonly float damageWRBoost = 5;
        static readonly float critWRBoost = 5;

        static readonly float setBonusWRDamage = 10;
        static readonly float setBonusRangedCrit = 10;
        static readonly float setBonusMeleeSpeedBoost = 10;

        static readonly float setBonusDefense = 20;
        static readonly float setBonusDR = 15;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageWRBoost, critWRBoost);
        public static LocalizedText setBonusText;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;
            setBonusText = this.GetLocalization("SetBonus").WithFormatArgs(setBonusWRDamage, setBonusRangedCrit, setBonusMeleeSpeedBoost, setBonusDefense, setBonusDR);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.defense = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Melee) += damageWRBoost / 100;
            player.GetDamage(DamageClass.Ranged) += damageWRBoost / 100;
            player.GetCritChance(DamageClass.Melee) += critWRBoost;
            player.GetCritChance(DamageClass.Ranged) += critWRBoost;

            player.GetModPlayer<ParacosmicHelmetPlayer>().ParacosmicHelmet = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ParacosmicChestplate>() && legs.type == ModContent.ItemType<ParacosmicLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<ParacosmicHelmetPlayer>().ParacosmicHelmetSet = true;
            player.setBonus = setBonusText.Value;
            player.GetDamage(DamageClass.Melee) += setBonusWRDamage / 100;
            player.GetDamage(DamageClass.Ranged) += setBonusWRDamage / 100;
            player.GetCritChance(DamageClass.Ranged) += setBonusRangedCrit;
            player.GetAttackSpeed(DamageClass.Melee) += setBonusMeleeSpeedBoost / 100;
        }
    }

    public class ParacosmicHelmetPlayer : ModPlayer
    {
        public bool ParacosmicHelmet = false;
        public bool ParacosmicHelmetSet = false;

        public override void ResetEffects()
        {
            ParacosmicHelmet = false;
            ParacosmicHelmetSet = false;
        }

        public override void PostUpdateEquips()
        {
            if (ParacosmicHelmet == false)
            {
                return;
            }

            if (ParacosmicHelmetSet == false)
            {
                return;
            }

            if (Player.statLife <= (Player.statLifeMax2 / 2))
            {
                Player.AddBuff(ModContent.BuffType<ParacosmicHelmetBuff>(), 10);
            }

            if (Player.statLife > (Player.statLifeMax2 / 2))
            {
                Player.ClearBuff(ModContent.BuffType<ParacosmicHelmetBuff>());
            }
        }
    }
}
