using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class BranchedOfLifedBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().branchedOfLifed = true;
        }
    }
}
