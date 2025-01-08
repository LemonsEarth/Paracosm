using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class VoidTremor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ParacosmRarity.DarkGray;
            Item.value = Item.sellPrice(0, 30);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Item116;
            Item.autoReuse = true;
            Item.damage = 450;
            Item.knockBack = 6;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Melee;
            Item.shootSpeed = 60;
            Item.shoot = ModContent.ProjectileType<VoidTremorProj>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                velocity *= Main.rand.NextFloat(0.75f, 1.5f);
                velocity = velocity.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-5, 5)));
                Projectile.NewProjectile(source, position, velocity, type, damage, 1f, player.whoAmI, 10, 20);
            }
            return false;
        }
    }
}
