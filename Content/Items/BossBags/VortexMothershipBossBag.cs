using Paracosm.Content.Items.Armor.Celestial;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Melee;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.BossBags
{
    public class VortexMothershipBossBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Item.type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Item.type] = false;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentVortex, 1, 15, 30));
            itemLoot.Add(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            itemLoot.Add(ItemDropRule.OneFromOptions(1, ModContent.ItemType<HorizonSplitter>(), ModContent.ItemType<TheCrucible>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ChampionsCrown>(), 1, 1, 1));
        }
    }
}
