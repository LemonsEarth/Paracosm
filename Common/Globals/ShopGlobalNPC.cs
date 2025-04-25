using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Ranged;
using Terraria;
using Terraria.GameContent.ItemDropRules;
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
                Condition downedEvilBoss = new Condition("Downed Evil Boss", () => NPC.downedBoss2);
                shop.Add(ModContent.ItemType<ChainsawGun>(), downedEvilBoss);
                shop.Add(ModContent.ItemType<CorruptStaff>(), downedEvilBoss);
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
