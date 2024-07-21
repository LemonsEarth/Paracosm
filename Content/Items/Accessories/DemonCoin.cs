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
    public class DemonCoin : ModItem
    {
        static float damageBoost = 0;
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
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DemonCoinPlayer>().demonCoin = true;
        }

        public class DemonCoinPlayer : ModPlayer
        {
            public bool demonCoin = false;

            public override void ResetEffects()
            {
                demonCoin = false;
            }

            public override void PostUpdateEquips()
            {
                if (demonCoin == false)
                {
                    return;
                }

                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);

                Player.GetDamage(DamageClass.Generic) += damageBoost;
            }

            public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
            {
                if (demonCoin == false)
                {
                    return;
                }
                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);
                modifiers.FinalDamage *= 1f + (1f - damageBoost);
            }

            public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
            {
                if (demonCoin == false)
                {
                    return;
                }
                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);
                modifiers.FinalDamage *= 1f + (1f - damageBoost);
            }
        }
    }
}
