using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class CursedFlamesBorder : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CursedFlameHostile);
            AIType = ProjectileID.CursedFlameHostile;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
        }
    }
}
