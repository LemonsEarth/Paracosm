using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class CorruptBolt : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DiamondBolt);
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item43, Projectile.Center);
            }
            AITimer++;
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y);
        }
    }
}
