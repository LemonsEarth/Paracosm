using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class Solace : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 120;
            Item.DamageType = DamageClass.Melee;
            Item.width = 72;
            Item.height = 72;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 60);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<SolaceStar>();
            Item.shootSpeed = 6;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = Item.shoot;
            damage = (int)((float)Item.damage * 1f);
            velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * Item.shootSpeed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            useCounter++;

            if (useCounter == 3)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                }
                useCounter = 0;
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ModContent.ItemType<Sunblade>(), 1);
            recipe1.AddIngredient(ItemID.FragmentSolar, 12);
            recipe1.AddTile(TileID.LunarCraftingStation);
            recipe1.Register();
        }
    }
}
