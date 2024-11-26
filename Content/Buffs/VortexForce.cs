using Paracosm.Common.Globals;
using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class VortexForce : ModBuff
    {
        public override void SetStaticDefaults()
        {
            
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().vortexForce = true;
        }
    }
}
