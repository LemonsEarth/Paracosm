using Paracosm.Content.Items.Accessories;
using Terraria;
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
            if (shop.NpcType == NPCID.ArmsDealer)
            {
                shop.Add(ModContent.ItemType<SteelSight>(), Condition.DownedDeerclops);
            }
            if (shop.NpcType == NPCID.BestiaryGirl)
            {
                shop.Add(ItemID.Burger, Condition.DownedPlantera);
            }
        }
    }
}
