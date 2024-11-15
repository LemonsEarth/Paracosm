using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class DefiledGlaive : ModItem
    {
        int useCounter = 0;
        int damageBoost = 1;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 30);

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.damage = 110;
            Item.knockBack = 6;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Melee;
            Item.shootSpeed = 4;
            Item.shoot = ModContent.ProjectileType<DefiledGlaiveProjectile>();
        }

        public override bool CanUseItem(Player player)
        {
            // Ensures no more than one spear can be thrown out, use this when using autoReuse
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                if (useCounter < 3)
                {
                    Projectile.NewProjectile(source, position, velocity, Item.shoot, damage * damageBoost, knockback, ai0: 0);
                    if (damageBoost > 1)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Dust.NewDust(position, 2, 2, DustID.OrangeTorch, velocity.X, velocity.Y, Scale: 3f);
                        }
                    }
                    damageBoost = 1;
                    useCounter++;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, ai0: 1);
                    SoundEngine.PlaySound(SoundID.Item4, player.MountedCenter);
                    damageBoost = 10;
                    useCounter = 0;
                }
            }
            return false;
        }

        public override bool? UseItem(Player player)
        {
            // Because we're skipping sound playback on use animation start, we have to play it ourselves whenever the item is actually used.
            if (!Main.dedServ && Item.UseSound.HasValue)
            {
                SoundEngine.PlaySound(Item.UseSound.Value, player.MountedCenter);
            }

            return null;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.TheRottedFork, 1);
            recipe1.AddIngredient(ModContent.ItemType<DivineFlesh>(), 16);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();
        }
    }
}
