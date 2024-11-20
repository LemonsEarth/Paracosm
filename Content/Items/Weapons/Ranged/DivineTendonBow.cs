using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class DivineTendonBow : ModItem
    {
        int useCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 160;
            Item.knockBack = 2f;
            Item.crit = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 28;
            Item.height = 44;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 20);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = 10;
            Item.shootSpeed = 100;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter + (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (useCounter % 3 == 0)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Projectile.NewProjectile(source, position, velocity.RotatedBy(i * MathHelper.PiOver4) / 10, ModContent.ProjectileType<BloodseekerArrow>(), damage, knockback / 3, ai0: 45);
                    }
                }
            }
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: 25);

            useCounter++;
            if (useCounter >= 30)
            {
                useCounter = 0;
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TendonBow, 1);
            recipe.AddIngredient(ModContent.ItemType<DivineFlesh>(), 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
