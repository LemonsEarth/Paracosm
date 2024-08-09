using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Paracosm.Content.Items.Materials
{
    public class NightmareScale : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.sellPrice(0, 0, 2);
            Item.rare = ItemRarityID.LightPurple;
        }
    }
}
