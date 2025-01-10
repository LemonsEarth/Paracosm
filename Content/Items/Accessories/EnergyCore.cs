using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Paracosm.Content.Items.Accessories
{
    public class EnergyCore : ModItem
    {
        static readonly float damageBoost = 22;
        static readonly float DRBoost = 22;
        int timer = 0;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, DRBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.defense = 6;
            Item.value = Item.sellPrice(0, 16);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.statLife <= player.statLifeMax2 / 2f)
            {
                player.endurance += DRBoost / 100f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += damageBoost / 100f;
            }
            player.noKnockback = true;

            if (Math.Abs(player.velocity.X) > 4.5f)
            {
                if (Main.myPlayer == player.whoAmI && timer == 30)
                {
                    SoundEngine.PlaySound(SoundID.Item69, player.Center);
                    Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, (player.velocity - (Vector2.UnitY)) * 4, ModContent.ProjectileType<Paraboulder>(), 2000, 1f, player.whoAmI);
                    timer = 0;
                }
            }

            if (timer < 30)
            {
                timer++;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<RollerShield>());
            recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 6);
            recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 4);
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 13);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
