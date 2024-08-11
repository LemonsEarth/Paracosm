using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;
using Paracosm.Content.Projectiles.Hostile;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons.Melee;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class ShadowBomber : ModItem
    {
        public override void SetStaticDefaults()
        {
            AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;
            AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Add(Type, new Dictionary<int, int>
            {
                { ItemID.RocketIV, ProjectileID.RocketSnowmanI }
            });
        }

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;
            Item.shoot = 10;
            Item.useAmmo = AmmoID.Rocket;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * 40;
            velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * 30;
            type = ModContent.ProjectileType<ShadowOrbProj>();
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //t variables are useless, only used ammo is needed
            bool ammoChosen = player.PickAmmo(ContentSamples.ItemsByType[Type], out int AmmoType, out float t2, out int t3, out float t4, out int t5, false);
            if (ammoChosen == false)
            {
                return false;
            }
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: AmmoType);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.OnyxBlaster, 1);
            recipe1.AddIngredient(ModContent.ItemType<NightmareScale>(), 15);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();
        }
    }
}
