using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Paracosmic
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

            player.GetModPlayer<ParacosmPlayer>().paracosmicHelmet = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ParacosmicChestplate>() && legs.type == ModContent.ItemType<ParacosmicLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<ParacosmPlayer>().paracosmicHelmetSet = true;
            player.setBonus = setBonusText.Value;
            player.GetDamage(DamageClass.Melee) += setBonusWRDamage / 100;
            player.GetDamage(DamageClass.Ranged) += setBonusWRDamage / 100;
            player.GetCritChance(DamageClass.Ranged) += setBonusRangedCrit;
            player.GetAttackSpeed(DamageClass.Melee) += setBonusMeleeSpeedBoost / 100;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 8);
            recipe.AddIngredient(ModContent.ItemType<CosmicFlames>(), 6);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
