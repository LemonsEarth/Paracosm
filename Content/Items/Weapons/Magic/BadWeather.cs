using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;
using System.Collections.Generic;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class BadWeather : ModItem
    {
        int[] projectiles =
        {
            1, 2, 3, 4, 5, 14, 15, 20, 27, 36, 45, 48, 51, 54, 76, 77, 78, 88, 93, 94, 95, 103, 104, 114, 116, 118, 119, 120, 121, 122, 123, 124, 125, 126, 173, 242, 239
        };

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Magic;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 10;
            Item.shoot = ProjectileID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld + new Vector2(Main.rand.Next(-100, 100), -500);
            velocity = new Vector2(0, 30);
            type = projectiles[Main.rand.Next(0, projectiles.Length)];
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback);
                projectile.originalDamage = projectile.damage;
                projectile.DamageType = DamageClass.Magic;
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SoulofFlight, 10);
            recipe1.AddIngredient(ItemID.Feather, 4);
            recipe1.AddIngredient(ItemID.TitaniumBar, 8);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.SoulofFlight, 10);
            recipe2.AddIngredient(ItemID.Feather, 4);
            recipe2.AddIngredient(ItemID.AdamantiteBar, 8);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}