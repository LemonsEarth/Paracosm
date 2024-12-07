using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class NebulousPower : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().nebulousPower = true;
        }
    }
}
