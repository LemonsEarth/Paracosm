using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class EventHorizon : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 700;
            Item.DamageType = DamageClass.Magic;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.reuseDelay = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ParacosmRarity.PinkPurple;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 12;
            Item.shoot = ModContent.ProjectileType<EventHorizonSphere>();
            Item.shootSpeed = 10;
            Item.noMelee = true;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, ai1: i);
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ItemID.LunarFlareBook);
            r.AddIngredient(ItemID.MagnetSphere);
            r.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 8);
            r.AddIngredient(ItemID.FragmentNebula, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}