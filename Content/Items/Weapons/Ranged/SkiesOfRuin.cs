using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class SkiesOfRuin : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 90;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 38;
            Item.height = 66;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 20);
            Item.rare = ParacosmRarity.LightBlue;
            Item.UseSound = SoundID.Item9;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PureStarshot>();
            Item.shootSpeed = 25;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + new Vector2(Main.rand.NextFloat(-50, 50), Main.rand.NextFloat(-50, 50));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer)
            {
                return false;
            }
            if (useCounter < 60)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: 30);
                useCounter++;
            }
            else
            {
                for (int j = 1; j < 5; j++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int proj = Main.rand.NextFromList(ProjectileID.SuperStar, ProjectileID.StarCannonStar, type, ProjectileID.StarWrath);
                        Vector2 pos = Main.MouseWorld + (Vector2.UnitY * 800).RotatedBy(MathHelper.ToRadians(i * 72));
                        Projectile.NewProjectile(source, pos, (Main.MouseWorld - pos).SafeNormalize(Vector2.Zero) * ((Item.shootSpeed * 2) / j), proj, damage * 2, knockback, ai0: 90);
                    }
                }
                
                useCounter = 0;
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<StarStormbow>());
            recipe.AddIngredient(ItemID.SuperStarCannon);
            recipe.AddIngredient(ModContent.ItemType<PureStardust>(), 8);
            recipe.AddIngredient(ItemID.FragmentStardust, 12);
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
