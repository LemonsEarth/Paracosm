using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Summon
{
    public class StaffOfTheStarbearer : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 50;
            Item.mana = 10;
            Item.noMelee = true;
            Item.damage = 10;
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item105;
            Item.shoot = ModContent.ProjectileType<PureStarshot>();
            Item.shootSpeed = 5;
            Item.rare = ParacosmRarity.LightBlue;
            Item.value = Item.sellPrice(gold: 40);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer != player.whoAmI)
            {
                return false;
            }
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.minion || proj.sentry)
                {
                    Vector2 direction = proj.velocity.SafeNormalize(Vector2.Zero);
                    Projectile.NewProjectile(source, proj.Center, direction * Item.shootSpeed, type, damage + proj.damage, knockback, player.whoAmI, ai0: 30);
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ItemID.StardustCellStaff);
            r.AddIngredient(ItemID.AbigailsFlower);
            r.AddIngredient(ModContent.ItemType<PureStardust>(), 8);
            r.AddIngredient(ItemID.FragmentStardust, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}
