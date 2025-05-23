﻿using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Sentries;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Summon
{
    public class GiantShiverthornStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.mana = 10;
            Item.noMelee = true;
            Item.damage = 8;
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<GiantShiverthorn>();
            Item.rare = ItemRarityID.Blue;
            Item.value = 2000;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
            damage = Item.damage;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
                player.UpdateMaxTurrets();
                projectile.originalDamage = Item.damage;
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.BorealWood, 20);
            recipe1.AddIngredient(ItemID.Shiverthorn, 3);
            recipe1.AddIngredient(ItemID.FlinxFur, 3);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();
        }
    }
}
