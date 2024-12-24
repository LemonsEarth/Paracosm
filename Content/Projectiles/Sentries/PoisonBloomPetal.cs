using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class PoisonBloomPetal : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item17);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            AITimer++;
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, Projectile.velocity.X, Projectile.velocity.Y);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
        }
    }
}
