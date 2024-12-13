using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class ComboBreakerProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.penetrate = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.Opacity = 0f;
            }

            if (Projectile.timeLeft > 15)
            {
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, AITimer / 5);
            }
            else
            {
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 1 / Projectile.timeLeft);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity /= 1.1f;

            for (int i = 0; i < 2; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, Scale: 1f);
                dust.noGravity = true;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }

            AITimer++;
        }
    }
}
