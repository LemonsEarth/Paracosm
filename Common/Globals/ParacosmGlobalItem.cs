using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalItem : GlobalItem
    {
        public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            if (player.GetModPlayer<ParacosmPlayer>().nebulousPower)
            {
                ascentWhenFalling = 3f;
                ascentWhenRising = 0.5f;
                maxCanAscendMultiplier = 1f;
                maxAscentMultiplier = 3.5f;
                constantAscend = 0.135f;
            }
        }

        public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
        {
            if (player.GetModPlayer<ParacosmPlayer>().nebulousPower)
            {
                speed = 15f;
                acceleration = 1f;
            }
        }
    }
}
