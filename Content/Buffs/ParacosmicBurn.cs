using Paracosm.Common.Globals;
using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class ParacosmicBurn : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().paracosmicBurn = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ParacosmGlobalNPC>().paracosmicBurn = true;
        }
    }
}
