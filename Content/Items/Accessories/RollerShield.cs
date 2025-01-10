using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using System;
using Terraria.Audio;

namespace Paracosm.Content.Items.Accessories
{
    public class RollerShield : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        int timer = 0;
        public override void SetDefaults()
        {
            Item.defense = 6;
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 3);
            Item.rare = ItemRarityID.Purple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.noKnockback = true;
            if (Math.Abs(player.velocity.X) > 4.5f)
            {
                if (Main.myPlayer == player.whoAmI && timer == 60)
                {
                    SoundEngine.PlaySound(SoundID.Item69, player.Center);
                    Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, (player.velocity - (Vector2.UnitY * 2)) * 2, ModContent.ProjectileType<Paraboulder>(), 20, 1f, player.whoAmI);
                    timer = 0;
                }
            }

            if (timer < 60)
            {
                timer++;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<RoundShield>());
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 23);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
