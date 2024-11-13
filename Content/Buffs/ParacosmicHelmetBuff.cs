using Paracosm.Common.Players;
using Terraria;
using Terraria.ModLoader;
namespace Paracosm.Content.Buffs
{
    public class ParacosmicHelmetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmPlayer>().paracosmicHelmetBuff = true;
        }
    }
}
