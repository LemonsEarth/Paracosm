using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;
namespace Paracosm.Content.Buffs
{
    public class ParacosmicGogglesBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().paracosmicGogglesBuff = true;
        }
    }
}
