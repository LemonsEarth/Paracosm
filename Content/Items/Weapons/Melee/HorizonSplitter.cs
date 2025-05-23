﻿using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class HorizonSplitter : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 1000;
            Item.DamageType = DamageClass.Melee;
            Item.width = 100;
            Item.height = 100;
            Item.useTime = 30;
            Item.useAnimation = 30;
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
            Item.shoot = ModContent.ProjectileType<HorizonSplitterProj>();
            Item.shootSpeed = 6;
            Item.channel = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float mousePosRel = Main.MouseWorld.X > player.Center.X ? 1 : -1;
            Projectile.NewProjectile(source, player.MountedCenter + new Vector2(-mousePosRel * 20, -20), Vector2.Zero, type, damage, knockback, player.whoAmI, ai1: 1, ai2: mousePosRel);
            return false;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ItemID.LunarHamaxeSolar);
            r.AddIngredient(ItemID.LucyTheAxe);
            r.AddIngredient(ModContent.ItemType<SolarCore>(), 8);
            r.AddIngredient(ItemID.FragmentSolar, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}
