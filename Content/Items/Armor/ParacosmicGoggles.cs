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
    public class ParacosmicGoggles : ModItem
    {
        static readonly float damageMSBoost = 10;
        static readonly int minionBoost = 1;

        static readonly float setBonusMSDamage = 16;
        static readonly float setBonusMagicCrit = 12;
        static readonly int setBonusMana = 40;
        static readonly int setBonusMinionBoost = 2;
        static readonly int setBonusSentryBoost = 2;

        static readonly float setBonusDamage = 20;
        static readonly float setBonusManaUsage = 40;
        

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageMSBoost, minionBoost);
        public static LocalizedText setBonusText;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
            setBonusText = this.GetLocalization("SetBonus").WithFormatArgs(setBonusMSDamage, setBonusMagicCrit, setBonusMana, setBonusMinionBoost, setBonusSentryBoost ,setBonusDamage, setBonusManaUsage);
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 34;
            Item.defense = 3;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += damageMSBoost / 100;
            player.GetDamage(DamageClass.Summon) += damageMSBoost / 100;
            player.maxMinions += minionBoost;

            player.GetModPlayer<ParacosmicGogglesPlayer>().paracosmicGoggles = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ParacosmicChestplate>() && legs.type == ModContent.ItemType<ParacosmicLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<ParacosmicGogglesPlayer>().paracosmicGogglesSet = true;
            player.setBonus = setBonusText.Value;
            player.GetDamage(DamageClass.Magic) += setBonusMSDamage / 100;
            player.GetDamage(DamageClass.Summon) += setBonusMSDamage / 100;
            player.GetCritChance(DamageClass.Magic) += setBonusMagicCrit;
            player.statManaMax2 += setBonusMana;
            player.maxMinions += setBonusMinionBoost;
            player.maxTurrets += setBonusSentryBoost;
        }
    }

    public class ParacosmicGogglesPlayer : ModPlayer
    {
        public bool paracosmicGoggles = false;
        public bool paracosmicGogglesSet = false;

        public override void ResetEffects()
        {
            paracosmicGoggles = false;
            paracosmicGogglesSet = false;
        }

        public override void PostUpdateEquips()
        {
            if (paracosmicGoggles == false)
            {
                return;
            }

            if (paracosmicGogglesSet == false)
            {
                return;
            }
            if (Player.statLife <= (Player.statLifeMax2 / 2))
            {
                Player.AddBuff(ModContent.BuffType<ParacosmicGogglesBuff>(), 10);
            }

            if (Player.statLife > (Player.statLifeMax2 / 2))
            {
                Player.ClearBuff(ModContent.BuffType<ParacosmicGogglesBuff>());
            }
        }
    }
}
