using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Minions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Summon
{
    public class RogueDefender : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 2f;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.mana = 10;
            Item.noMelee = true;
            Item.damage = 240;
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.UseSound = SoundID.Zombie67;
            Item.buffType = ModContent.BuffType<TeslaGunMinionBuff>();
            Item.shoot = ModContent.ProjectileType<TeslaGunMinion>();
            Item.rare = ParacosmRarity.LightGreen;
            Item.value = Item.sellPrice(gold: 40);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            if (Main.myPlayer == player.whoAmI)
            {
                var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
                projectile.originalDamage = Item.damage;
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient(ItemID.XenoStaff);
            r.AddIngredient(ItemID.DD2LightningAuraT3Popper);
            r.AddIngredient(ModContent.ItemType<VortexianPlating>(), 8);
            r.AddIngredient(ItemID.FragmentVortex, 12);
            r.AddIngredient(ItemID.LunarBar, 8);
            r.Register();
        }
    }
}
