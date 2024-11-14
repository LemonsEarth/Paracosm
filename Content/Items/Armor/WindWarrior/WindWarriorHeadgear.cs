using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Armor.WindWarrior
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
