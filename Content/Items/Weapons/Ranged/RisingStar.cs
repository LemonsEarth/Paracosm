using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class RisingStar : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.knockBack = 2f;
            Item.crit = 2;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 80;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item9;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Starshot>();
            Item.shootSpeed = 30;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + new Vector2(Main.rand.NextFloat(-50, 50), Main.rand.NextFloat(-50, 50));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: 30);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Starfury);
            recipe.AddIngredient(ItemID.ShadowScale, 15);
            recipe.AddIngredient(ItemID.FallenStar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.Starfury);
            recipe1.AddIngredient(ItemID.TissueSample, 15);
            recipe1.AddIngredient(ItemID.FallenStar, 10);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();
        }
    }
}
