using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Weapons.Melee;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Ranged;
using Paracosm.Content.Items.Weapons.Summon;

namespace Paracosm.Content.Items.BossBags
{
    public class InfectedRevenantBossBag : ModItem
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
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<NightmareScale>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DivineFlesh>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedDragonHeart>(), 1, 1, 1));
        }
    }
}
