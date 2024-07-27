using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;
using Paracosm.Content.Projectiles.Hostile;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria.Audio;

namespace Paracosm.Content.Items.Weapons
{
    public class FocusBow : ModItem
    {
        float speed = 1;

        public override void SetDefaults()
        {
            Item.damage = 180;
            Item.knockBack = 2f;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 58;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 20);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = 10;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero);
            velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * speed;
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                speed += 2;
                if (Item.useTime > 6)
                {
                    Item.useTime -= 2;
                    Item.useAnimation -= 2;
                }
                Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity, type, damage, knockback);
                if (speed > 24)
                {
                    Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity / 1.2f, type, damage, knockback);
                    Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity / 1.4f, type, damage, knockback);
                }
                if (speed >= 30)
                {
                    speed = 1;
                    Item.useTime = 30;
                    Item.useAnimation = 30;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShroomiteBar, 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
