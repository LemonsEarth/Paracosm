﻿using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class ParacosmicFurnace : ModItem
    {
        int useCounter = 0;
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(9, 3));
        }

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 20;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.3f;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ParacosmicFlame>();
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * 40;
            velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * 30;
            type = ModContent.ProjectileType<ParacosmicFlame>();
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians((float)Math.Sin(useCounter) / 3 * 30 * 1)), type, damage, knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians((float)Math.Sin(useCounter) / 3 * 30 * -1)), type, damage, knockback, player.whoAmI);
                if (useCounter % 10 == 0)
                {
                    Projectile.NewProjectile(source, position, velocity / 3, ModContent.ProjectileType<HomingBlueFire>(), damage * 2, knockback, player.whoAmI);
                    Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.PiOver4) / 3, ModContent.ProjectileType<HomingBlueFire>(), damage * 2, knockback, player.whoAmI);
                    Projectile.NewProjectile(source, position, velocity.RotatedBy(-MathHelper.PiOver4) / 3, ModContent.ProjectileType<HomingBlueFire>(), damage * 2, knockback, player.whoAmI);
                }
            }
            useCounter++;
            if (useCounter == 30)
            {
                useCounter = 0;
            }
            return false;
        }
    }
}
