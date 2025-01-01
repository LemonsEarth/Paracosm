using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class Sunblade : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Melee;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ProjectileID.HallowStar;
            Item.shootSpeed = 10;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = Item.shoot;
            damage = (int)((float)Item.damage * 0.75f);
            velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * Item.shootSpeed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15))) * Main.rand.NextFloat(0.8f, 1.2f), type, damage, knockback, player.whoAmI);
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.PlatinumBroadsword, 1);
            recipe1.AddIngredient(ItemID.FallenStar, 7);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.GoldBroadsword, 1);
            recipe2.AddIngredient(ItemID.FallenStar, 7);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();
        }
    }
}
