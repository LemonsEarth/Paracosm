using Paracosm.Common.Globals;
using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs
{
    public class StardustTailDebuff : ModBuff
    {
        public static readonly int TagDamageBoost = 60;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
