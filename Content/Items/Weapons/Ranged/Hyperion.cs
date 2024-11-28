using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class Hyperion : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.crit = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 146;
            Item.height = 68;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.3f;
            Item.value = Item.sellPrice(0, 15);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item38;
            Item.autoReuse = true;
            Item.shootSpeed = 10;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 offset = velocity.SafeNormalize(Vector2.Zero) * 40;
            offset.Y += 15;
            position += offset;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                useCounter++;
                for (int i = -3; i < 3; i++)
                {
                    Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity.RotatedBy(MathHelper.ToRadians(3 * i)), type, damage, knockback);
                }

                if (useCounter == 10)
                {
                    for (int i = -4; i <= 4; i++)
                    {
                        Projectile.NewProjectile(Item.GetSource_FromAI(), position, velocity.RotatedBy(MathHelper.ToRadians(10 * i)) * 2, ProjectileID.VortexBeaterRocket, damage * 4, knockback);
                    }
                    useCounter = 0;
                }
            }
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-60, 6);
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ItemID.Megashark);
            r.AddIngredient(ItemID.VortexBeater);
            r.AddIngredient(ModContent.ItemType<VortexianPlating>(), 8);
            r.AddIngredient(ItemID.FragmentVortex, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}
