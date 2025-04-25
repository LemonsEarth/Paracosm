using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class VoidcoreStaff : ModItem
    {
        public int chargeAmount { get; set; } = 0;
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Magic;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ParacosmRarity.DarkGray;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.mana = 12;
            Item.shoot = ModContent.ProjectileType<VoidBolt>();
            Item.shootSpeed = 10;
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //if (Main.myPlayer == player.whoAmI)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        int projID = ProjectileID.TinyEater;
            //        if (i == Main.rand.Next(0, 4))
            //        {
            //            projID = ModContent.ProjectileType<ArchmageBolt>();
            //        }
            //        Projectile.NewProjectile(source, Main.MouseWorld, Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i) * 20, type, damage, knockback, player.whoAmI, ai0: projID, ai2: 1);
            //    }
            //}
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (chargeAmount > 20)
            {
                if (chargeAmount >= 100) LemonUtils.DustCircle(player.Center, 8, 10, DustID.Granite, 3f);
                if (player.altFunctionUse == 2)
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        if (Main.myPlayer == player.whoAmI)
                        {
                            Vector2 direction = player.Center.DirectionTo(Main.MouseWorld).RotatedBy(MathHelper.ToRadians(i * 15)) * 20;
                            Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, direction, ModContent.ProjectileType<SuperVoidBolt>(), (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(Item.damage * 1), 1f, player.whoAmI, 2, 5);
                        }
                    }
                    chargeAmount -= 20;
                }
            }
            if (player.altFunctionUse == 0)
            {
                return null;
            }

            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ModContent.ItemType<ArchmageStaff>(), 1);
            recipe1.AddIngredient(ModContent.ItemType<CorruptStaff>(), 1);
            recipe1.AddIngredient(ModContent.ItemType<VoidEssence>(), 16);
            recipe1.AddIngredient(ItemID.LunarBar, 8);
            recipe1.AddTile(TileID.LunarCraftingStation);
            recipe1.Register();
        }
    }
}