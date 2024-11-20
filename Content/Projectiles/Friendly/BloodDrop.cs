using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class BloodDrop : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 15;
            }
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            var dust = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.RedTorch);
            Projectile.velocity *= 1.01f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, 16, 16, DustID.CrimsonTorch);
            }
        }
    }
}
