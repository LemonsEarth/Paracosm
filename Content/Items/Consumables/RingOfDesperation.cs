using Paracosm.Content.Bosses.NebulaMaster;
using Paracosm.Content.Bosses.TheNameless;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Consumables
{
    public class RingOfDesperation : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = ParacosmRarity.DarkGray;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<TheNameless>());
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnBoss((int)player.MountedCenter.X + 2500, (int)player.MountedCenter.Y - 2500, ModContent.NPCType<TheNameless>(), player.whoAmI);
            }
            else
            {
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: Type);
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CelestialSigil, 1);
            recipe.AddIngredient(ModContent.ItemType<SolarCore>(), 10);
            recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 10);
            recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 10);
            recipe.AddIngredient(ModContent.ItemType<PureStardust>(), 10);
            recipe.AddIngredient(ModContent.ItemType<VoidEssence>(), 30);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
