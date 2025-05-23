﻿using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class GravityBarrage : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 42;
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 6;
            Item.shoot = ModContent.ProjectileType<ParaSwordShard>();
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + new Vector2(Main.rand.Next(-100, 100), 600 + Main.rand.Next(-20, 20));
            velocity = new Vector2(0, -30);
            type = Item.shoot;
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: 1);
                }
            }
            return false;
        }
    }
}