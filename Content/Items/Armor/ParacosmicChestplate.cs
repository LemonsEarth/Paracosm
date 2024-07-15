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
    public class ParacosmicChestplate : ModItem
    {
        static readonly float damageBoost = 5;
        static readonly float critBoost = 8;
        static readonly float drBoost = 7;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, drBoost);

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 34;
            Item.defense = 20;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 8, 0, 0);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.endurance += drBoost / 100;
        }
    }
}
