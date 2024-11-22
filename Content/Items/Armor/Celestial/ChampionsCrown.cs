using Microsoft.Xna.Framework;
using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.Celestial
{
    [AutoloadEquip(EquipType.Head)]
    public class ChampionsCrown : ModItem
    {
        static readonly float meleeDamageBoost = 15;
        static readonly float meleeCritBoost = 20;
        static readonly float meleeSpeedBoost = 10;
        static readonly int lifeRegenBoost = 2;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(meleeDamageBoost, meleeCritBoost, meleeSpeedBoost, lifeRegenBoost);
        public static LocalizedText setBonusText;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;
            setBonusText = this.GetLocalization("SetBonus");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.defense = 18;
            Item.lifeRegen = lifeRegenBoost;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 15, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Melee) += meleeDamageBoost / 100;
            player.GetCritChance(DamageClass.Melee) += meleeCritBoost;
            player.GetAttackSpeed(DamageClass.Melee) += meleeSpeedBoost / 100;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemID.SolarFlareBreastplate && legs.type == ItemID.SolarFlareLeggings;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = setBonusText.Value;
            player.GetModPlayer<ParacosmPlayer>().championsCrownSet = true;
        }

        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
        {
            color = Color.White;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SoulofFlight, 10);
            recipe1.AddIngredient(ItemID.Feather, 2);
            recipe1.AddIngredient(ItemID.CobaltBar, 5);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.SoulofFlight, 10);
            recipe2.AddIngredient(ItemID.Feather, 2);
            recipe2.AddIngredient(ItemID.PalladiumBar, 5);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}
