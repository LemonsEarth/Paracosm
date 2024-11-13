using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class TheGuardDuty : ModItem
    {
        Vector2 shootPos = Vector2.Zero;
        int useCounter = 0;

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 24;
            Item.useTime = 3;
            Item.useAnimation = 3;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.3f;
            Item.value = Item.sellPrice(0, 15);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.HornetStinger;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ProjectileID.HornetStinger;
            position = player.MountedCenter + (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * 35;
            shootPos = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15)));
            velocity = shootPos * 100;
            damage = Item.damage;
            knockback = Item.knockBack;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(shootPos, 2, 2, DustID.Torch, 0, 0);
            }
            if (Main.myPlayer == player.whoAmI)
            {
                useCounter++;
                Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity, type, damage, knockback);

                if (useCounter == 20)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int beeID = player.beeType();
                        int beeDamage = player.beeDamage(damage * 3);
                        Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity / Main.rand.Next(10, 20), beeID, beeDamage, knockback);
                    }
                    SoundEngine.PlaySound(SoundID.Item97 with { Volume = 0.7f });
                    useCounter = 0;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Minishark, 1);
            recipe.AddIngredient(ItemID.Stinger, 12);
            recipe.AddIngredient(ItemID.BeeWax, 16);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
