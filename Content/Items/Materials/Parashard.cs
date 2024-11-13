using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Materials
{
    public class Parashard : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.sellPrice(0, 0, 2);
            Item.rare = ItemRarityID.Purple;
        }
    }
}
