using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;

namespace Paracosm.Content.Projectiles.Minions
{
    public class CorruptDragonBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CorruptDragon>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
