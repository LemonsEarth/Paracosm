using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class TheCrucible : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 233;
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 38;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ParacosmRarity.Orange;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 15;
            Item.shoot = ModContent.ProjectileType<TheCrucibleProj>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 mousePos = (Main.MouseWorld - player.MountedCenter);
            position = player.Center + new Vector2(mousePos.X * 60, -40);
            velocity = Vector2.Zero;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ModContent.ItemType<OccultFlames>());
            r.AddIngredient(ItemID.FlowerofFire);
            r.AddIngredient(ModContent.ItemType<SolarCore>(), 8);
            r.AddIngredient(ItemID.FragmentSolar, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}