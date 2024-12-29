using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Projectiles.HeldProjectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class Equinox : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 75;
            Item.DamageType = DamageClass.Magic;
            Item.width = 58;
            Item.height = 54;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.reuseDelay = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 60);
            Item.rare = ParacosmRarity.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<EquinoxSun>();
            Item.shootSpeed = 6;
            Item.channel = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        { 
            int sunID = Projectile.NewProjectile(source, player.MountedCenter - Vector2.UnitY * 45.25f, Vector2.Zero, type, damage, knockback);
            Projectile.NewProjectile(source, player.MountedCenter + Vector2.UnitY * 33.94f, Vector2.Zero, ModContent.ProjectileType<EquinoxMoon>(), damage, knockback, ai0: sunID);
            return false;
        }
    }
}
