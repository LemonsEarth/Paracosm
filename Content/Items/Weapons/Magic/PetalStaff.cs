using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class PetalStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item17;
            Item.autoReuse = true;
            Item.mana = 5;
            Item.shoot = ModContent.ProjectileType<PoisonPetal>();
            Item.shootSpeed = 12;
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15))) * Main.rand.NextFloat(0.8f, 1.2f), type, damage, knockback);
                    }
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.Vine, 2);
            recipe1.AddIngredient(ItemID.Stinger, 6);
            recipe1.AddIngredient(ItemID.JungleSpores, 8);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();
        }
    }
}