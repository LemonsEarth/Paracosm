using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class OccultFlames : ModItem
    {
        int useCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.crit = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2;
            Item.value = Item.buyPrice(gold: 40);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 8;
            Item.shoot = ProjectileID.CursedFlameFriendly;
            Item.shootSpeed = 12;
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-25, 25))) * Main.rand.NextFloat(0.5f, 2f), type, damage, knockback);
                }
                useCounter++;

                if (useCounter == 2)
                {
                    var proj = Projectile.NewProjectileDirect(source, position, velocity / 10, ModContent.ProjectileType<CursedSpiritFlameFriendly>(), damage, knockback, player.whoAmI, ai0: 20);
                    CursedSpiritFlameFriendly csf = (CursedSpiritFlameFriendly)proj.ModProjectile;
                    csf.speed = 10;
                    useCounter = 0;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.CursedFlames, 1);
            recipe1.AddIngredient(ModContent.ItemType<NightmareScale>(), 12);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();
        }
    }
}