using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;

namespace Paracosm.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class WindWarriorBreastplate : ModItem
    {
        static readonly float manaCostReduction = 20;
        static readonly float damageBoost = 10;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, manaCostReduction);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.defense = 10;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 7, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.manaCost -= manaCostReduction / 100;
            player.GetModPlayer<WindWarriorBreastplatePlayer>().WindWarriorBreastplate = true;
        }
    }

    public class WindWarriorBreastplatePlayer : ModPlayer
    {
        public bool WindWarriorBreastplate = false;

        public override void ResetEffects()
        {
            WindWarriorBreastplate = false;
        }

        public override void PostUpdateEquips()
        {
            if (WindWarriorBreastplate == false)
            {
                return;
            }

            Player.wingTimeMax += Player.wingTimeMax / 2;
        }
    }
}
