using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Ranged
{
    public class FadedFates : ModItem
    {
        int useCounter = 0;

        public override void SetDefaults()
        {
            Item.damage = 75;
            Item.crit = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 44;
            Item.height = 34;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ParacosmRarity.DarkGray;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FadedFatesProj>();
            Item.shootSpeed = 18;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ModContent.ItemType<AssassinsBackup>());
            recipe1.AddIngredient(ItemID.BoneDagger, 300);
            recipe1.AddIngredient(ModContent.ItemType<VoidEssence>(), 20);
            recipe1.AddTile(TileID.LunarCraftingStation);
            recipe1.Register();
        }
    }
}