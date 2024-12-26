using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class AssassinsBackup : ModItem
    {
        int useCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.crit = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AssassinsBackupProj>();
            Item.shootSpeed = 18;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (useCounter >= 5)
                {
                    Vector2 mouseDir = player.Center.DirectionTo(Main.MouseWorld);

                    for (int i = -4; i < 4; i++)
                    {
                        Projectile.NewProjectile(source, position, mouseDir.RotatedBy(i * MathHelper.ToRadians(180f / 8f)) * Item.shootSpeed, type, damage, knockback, ai1: 1);
                    }
                    useCounter = 0;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback);
                    useCounter++;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.ThrowingKnife, 50);
            recipe1.AddIngredient(ItemID.SoulofFright, 5);
            recipe1.AddIngredient(ItemID.AdamantiteBar, 6);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.ThrowingKnife, 50);
            recipe2.AddIngredient(ItemID.SoulofFright, 5);
            recipe2.AddIngredient(ItemID.TitaniumBar, 6);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}