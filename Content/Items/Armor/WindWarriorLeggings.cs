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
    [AutoloadEquip(EquipType.Legs)]
    public class WindWarriorLeggings : ModItem
    {
        static readonly float moveSpeedBoost = 75;
        static readonly int critBoost = 8;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(critBoost, moveSpeedBoost);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.defense = 6;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.moveSpeed += (float)moveSpeedBoost / 100;
            player.jumpSpeedBoost = 4;
            player.GetModPlayer<WindWarriorLeggingsPlayer>().WindWarriorLeggings = true;
        }
    }

    public class WindWarriorLeggingsPlayer : ModPlayer
    {
        public bool WindWarriorLeggings = false;

        public override void ResetEffects()
        {
            WindWarriorLeggings = false;
        }

        public override void PostUpdateEquips()
        {
            if (WindWarriorLeggings == false)
            {
                return;
            }
        }
    }
}
