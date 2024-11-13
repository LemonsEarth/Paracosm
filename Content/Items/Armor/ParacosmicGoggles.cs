using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
            setBonusText = this.GetLocalization("SetBonus").WithFormatArgs(setBonusMSDamage, setBonusMagicCrit, setBonusMana, setBonusMinionBoost, setBonusSentryBoost, setBonusDamage, setBonusManaUsage);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 16;
            Item.defense = 3;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += damageMSBoost / 100;
            player.GetDamage(DamageClass.Summon) += damageMSBoost / 100;
            player.maxMinions += minionBoost;

            player.GetModPlayer<ParacosmPlayer>().paracosmicGoggles = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ParacosmicChestplate>() && legs.type == ModContent.ItemType<ParacosmicLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<ParacosmPlayer>().paracosmicGogglesSet = true;
            player.setBonus = setBonusText.Value;
            player.GetDamage(DamageClass.Magic) += setBonusMSDamage / 100;
            player.GetDamage(DamageClass.Summon) += setBonusMSDamage / 100;
            player.GetCritChance(DamageClass.Magic) += setBonusMagicCrit;
            player.statManaMax2 += setBonusMana;
            player.maxMinions += setBonusMinionBoost;
            player.maxTurrets += setBonusSentryBoost;
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
