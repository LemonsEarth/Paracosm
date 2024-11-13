using Paracosm.Content.Items.Accessories;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ShopGlobalNPC : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Merchant)
            {
                shop.Add(ModContent.ItemType<SunCoin>());
            }
        }
    }
}
