using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class ScarletRod : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item66;
            Item.autoReuse = true;
            Item.mana = 20;
            Item.shoot = ModContent.ProjectileType<ScarletCloudMoving>();
            Item.shootSpeed = 10;
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(source, position, velocity, Item.shoot, Item.damage, 1, ai1: Main.MouseWorld.X, ai2: Main.MouseWorld.Y);
            }
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.CrimsonRod, 1);
            recipe1.AddIngredient(ModContent.ItemType<DivineFlesh>(), 10);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();
        }
    }
}