using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.BossBags
{
    public class NebulaMasterBossBag : ModItem
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
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<UnstableNebulousFlame>(), 1, 5, 14));
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentNebula, 1, 15, 30));
            itemLoot.Add(ItemDropRule.Common(ItemID.LunarBar, 1, 5, 12));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NebulousEnergy>(), 1, 1, 1));
        }
    }
}
