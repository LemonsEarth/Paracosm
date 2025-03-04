﻿using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class ArchmageStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.mana = 22;
            Item.noMelee = true;
            Item.damage = 120;
            Item.DamageType = DamageClass.Magic;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item43;
            Item.rare = ItemRarityID.Yellow;
            Item.value = 100000;
            Item.shoot = ModContent.ProjectileType<ArchmageBolt>();
            Item.shootSpeed = 5;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShadowbeamStaff);
            recipe.AddIngredient(ItemID.InfernoFork);
            recipe.AddIngredient(ItemID.SpectreStaff);
            recipe.AddIngredient(ModContent.ItemType<NightmareScale>(), 6);
            recipe.AddIngredient(ModContent.ItemType<DivineFlesh>(), 6);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
