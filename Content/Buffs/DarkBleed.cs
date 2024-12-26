using Paracosm.Common.Globals;
using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class DarkBleed : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ParacosmGlobalNPC>().darkBleed = true;
        }
    }
}
