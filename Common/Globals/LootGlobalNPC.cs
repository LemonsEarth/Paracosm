using Paracosm.Content.Items.Accessories;
using Paracosm.Content.Items.Weapons.Magic;
using Paracosm.Content.Items.Weapons.Summon;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class LootGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                // Demon Coins
                case NPCID.VoodooDemon:
                    npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<DemonCoin>(), 5, 4));
                    break;
                case NPCID.Demon:
                    npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<DemonCoin>(), 20, 10));
                    break;
                case NPCID.Tim:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), 1, 1, 3));
                    break;
                case NPCID.WallofFlesh or NPCID.BloodNautilus:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), minimumDropped: 3, maximumDropped: 6));
                    break;
                case NPCID.Mimic or NPCID.GoblinShark:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>()));
                    break;
                case NPCID.RuneWizard:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), 1, 3, 5));
                    break;
                case NPCID.BigMimicCorruption or NPCID.BigMimicCrimson or NPCID.BigMimicHallow or NPCID.Moth or NPCID.SandElemental or NPCID.IceGolem or NPCID.BloodEelHead or NPCID.RainbowSlime or NPCID.PirateShip:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), minimumDropped: 2, maximumDropped: 4));
                    break;

                case NPCID.GoblinSummoner:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShadowflowerStaff>(), 4));
                    break;
                case NPCID.Mothron:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Equinox>(), 4));
                    break;
            }
        }
    }
}
