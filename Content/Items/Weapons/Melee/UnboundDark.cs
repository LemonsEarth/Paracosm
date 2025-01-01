using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class UnboundDark : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 800;
            Item.DamageType = DamageClass.Melee;
            Item.width = 86;
            Item.height = 86;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ParacosmRarity.DarkGray;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<UnboundDarkProj>();
            Item.shootSpeed = 30;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] == 0;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                velocity = Vector2.Zero;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                int dir = useCounter % 2 == 0 ? 1 : -1;
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: dir, ai1: Main.MouseWorld.X, ai2: Main.MouseWorld.Y);
                useCounter++;
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ModContent.ItemType<LightsEnd>(), 1);
            recipe1.AddIngredient(ModContent.ItemType<VoidEssence>(), 16);
            recipe1.AddIngredient(ItemID.LunarBar, 12);
            recipe1.AddTile(TileID.LunarCraftingStation);
            recipe1.Register();
        }
    }
}
