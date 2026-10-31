using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs;

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
