using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles;
using Terraria.DataStructures;

namespace Paracosm.Content.Items.Weapons
{
    public class Uragan : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Melee;
            Item.width = 80;
            Item.height = 80;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<WindSlash>();
            Item.channel = true;
            Item.UseSound = SoundID.Item60;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-150, 150));
            type = ModContent.ProjectileType<WindSlash>();
            damage = Item.damage / 2;
            knockback = Item.knockBack / 2;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback);
            }
            return false;
        }
    }
}
