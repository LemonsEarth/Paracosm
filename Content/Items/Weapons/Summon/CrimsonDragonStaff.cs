using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;
using Paracosm.Content.Projectiles.Minions;
using Paracosm.Content.Items.Materials;

namespace Paracosm.Content.Items.Weapons.Summon
{
    public class CrimsonDragonStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.mana = 10;
            Item.noMelee = true;
            Item.damage = 120;
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item44;
            Item.buffType = ModContent.BuffType<CrimsonDragonBuff>();
            Item.shoot = ModContent.ProjectileType<CrimsonDragon>();
            Item.rare = ItemRarityID.LightRed;
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
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ModContent.ItemType<DivineFlesh>(), 10);
            recipe1.AddTile(TileID.MythrilAnvil);
            recipe1.Register();
        }
    }
}
