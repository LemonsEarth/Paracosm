using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Terraria.DataStructures;
using System;
using Terraria.WorldBuilding;

namespace Paracosm.Content.Items.Accessories
{
    public class SunCoin : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Red;
            Item.isAShopItem = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SunCoinPlayer>().sunCoin = true;
        }

    }
    public class SunCoinPlayer : ModPlayer
    {
        public bool sunCoin = false;

        public override void ResetEffects()
        {
            sunCoin = false;
        }

        public override void PostUpdateEquips()
        {
            if (sunCoin == false)
            {
                return;
            }

            Player.AddBuff(BuffID.Sunflower, 10);
        }
    }

    public class MerchantShop : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(ModContent.ItemType<SunCoin>());
            }
        }
    }
}
