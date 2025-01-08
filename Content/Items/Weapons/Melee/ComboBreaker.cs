using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class ComboBreaker : ModItem
    {
        int useCounter = 0;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 46;
            Item.rare = ParacosmRarity.PinkPurple;
            Item.value = Item.sellPrice(0, 50);
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.useAnimation = 6;
            Item.useTime = 6;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.damage = 220;
            Item.knockBack = 6;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Melee;
            Item.shootSpeed = 20;
            Item.shoot = ModContent.ProjectileType<ComboBreakerProj>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(source, position + velocity.SafeNormalize(Vector2.Zero) * 2, velocity.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15))) * Main.rand.NextFloat(0.5f, 1.5f), type, damage, knockback, player.whoAmI);
                }
                if (useCounter < 100) useCounter++;
            }
            if (useCounter >= 100)
            {
                LemonUtils.DustCircle(player.Center, 16, 10, DustID.GemAmethyst);
                SoundEngine.PlaySound(SoundID.Item172 with { MaxInstances = 1, PitchRange = (-0.2f, 0.2f), SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, player.Center);
            }
            return false;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
            {
                return true;
            }
            if (player.altFunctionUse == 2 && useCounter >= 100)
            {
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero), ModContent.ProjectileType<NebulaBeamFriendly>(), Item.damage * 10, 1f, player.whoAmI, ai0: 30);
                useCounter = 0;
            }
            return true;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FetidBaghnakhs, 1);
            recipe.AddIngredient(ItemID.SlapHand, 1);
            recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 12);
            recipe.AddIngredient(ItemID.FragmentNebula, 12);
            recipe.AddIngredient(ItemID.LunarBar, 8);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
