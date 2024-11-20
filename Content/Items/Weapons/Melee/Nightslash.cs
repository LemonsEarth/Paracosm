using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class Nightslash : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 60;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<NightActualSlash>();
            Item.channel = true;
            Item.UseSound = SoundID.Item60;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
            type = ModContent.ProjectileType<NightActualSlash>();
            damage = Item.damage / 2;
            knockback = Item.knockBack / 2;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(source, position + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-170, 170)), velocity, type, damage, knockback);
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.Katana, 1);
            recipe1.AddIngredient(ItemID.ShadowScale, 10);
            recipe1.AddIngredient(ModContent.ItemType<Parashard>(), 20);
            recipe1.AddTile(TileID.Anvils);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.Katana, 1);
            recipe2.AddIngredient(ItemID.TissueSample, 10);
            recipe2.AddIngredient(ModContent.ItemType<Parashard>(), 20);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();
        }
    }
}
