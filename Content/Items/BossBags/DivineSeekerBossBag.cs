using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Items.Accessories;

namespace Paracosm.Content.Items.BossBags
{
    public class DivineSeekerBossBag : ModItem
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
            Item.rare = ItemRarityID.Purple;
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Parashard>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.OneFromOptions(1, ModContent.ItemType<ParashardSword>(), ModContent.ItemType<ParacosmicFurnace>(), ModContent.ItemType<GravityBarrage>(), ModContent.ItemType<ParacosmicEyeStaff>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ParashardSigil>(), 1, 1, 1));
        }
    }
}
