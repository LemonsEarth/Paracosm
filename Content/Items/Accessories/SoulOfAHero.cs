using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Paracosm.Content.Bosses.SolarChampion;
using Paracosm.Content.Items.Materials;
using Paracosm.Common.Players;
using Paracosm.Content.Buffs;
using Terraria.DataStructures;

namespace Paracosm.Content.Items.Accessories
{
    public class SoulOfAHero : ModItem
    {
        int BuffImmuneType = ModContent.BuffType<VoidTerrorDebuff>();

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 3));
        }

        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 104;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 50);
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffImmuneType] = true;
            player.GetModPlayer<ParacosmPlayer>().heroSoul = true;
        }
    }     
}
