using Paracosm.Content.Bosses.SolarChampion;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Consumables
{
    public class PlayerVoodooDoll : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
        }

        public override bool? UseItem(Player player)
        {   
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.statLife > 20) player.statLife -= 20;
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.GuideVoodooDoll);
            recipe.AddIngredient(ItemID.FamiliarWig);
            recipe.AddIngredient(ItemID.FamiliarShirt);
            recipe.AddIngredient(ItemID.FamiliarPants);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
    }
}
