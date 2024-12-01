using Microsoft.Xna.Framework;
using Paracosm.Content.Bosses.VortexMothership;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Consumables
{
    public class DarkMagicMirror : ModItem
    {
        bool doAnimation = false;
        float animationTimer = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 68;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = 122;
            Item.useTime = 122;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
        }

        public override bool CanUseItem(Player player)
        {
            return !doAnimation;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item6 with { Pitch = -0.6f }, player.position);
            doAnimation = true;

            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (!doAnimation)
            {
                return;
            }
            for (int i = 0; i < (animationTimer % 20); i++)
            {
                Dust dust = Dust.NewDustDirect(player.Center, 1, 1, DustID.Granite, newColor: Color.Black, Scale: Main.rand.NextFloat(1.2f, 2.4f));
                dust.velocity = new Vector2(0, Main.rand.Next(1, 20)).RotatedByRandom(MathHelper.Pi * 2);
                dust.noGravity = true;
            }
            if (animationTimer > 120)
            {
                if (!SubworldSystem.AnyActive())
                {
                    SubworldSystem.Enter<VoidSubworld>();
                }
                doAnimation = false;
                animationTimer = 0;
            }
            animationTimer++;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 5);
            recipe.AddIngredient(ItemID.FragmentSolar, 5);
            recipe.AddIngredient(ItemID.FragmentVortex, 5);
            recipe.AddIngredient(ItemID.FragmentNebula, 5);
            recipe.AddIngredient(ItemID.FragmentStardust, 5);
            recipe.AddIngredient(ModContent.ItemType<SolarCore>(), 7);
            recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 7);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
