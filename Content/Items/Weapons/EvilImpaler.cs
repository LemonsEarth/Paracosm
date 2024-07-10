using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Mono.Cecil;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Utilities;
using Paracosm.Content.Projectiles;
using Terraria.Chat;
using Terraria.UI.Chat;
using Paracosm.Content.Items.Materials;

namespace Paracosm.Content.Items.Weapons
{
    public class EvilImpaler : ModItem
    {
        float counter = 0;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.scale = 2f;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 100;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.shootSpeed = 3.7f;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<EvilImpalerProjectile>();
            Item.useStyle = ItemUseStyleID.Shoot;
            ItemID.Sets.Spears[Item.type] = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SoulofNight, 5);
            recipe.AddIngredient(ItemID.TheRottedFork, 1);
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
