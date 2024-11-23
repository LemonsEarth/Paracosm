using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class TrackingTeslaShot : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float Target => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            DrawOffsetX = -20;
            DrawOriginOffsetX = 10;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.Opacity = 0;
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap);
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
                SoundEngine.PlaySound(SoundID.Item94 with { PitchRange = (0.5f, 0.7f) });
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueMoss);
            Lighting.AddLight(Projectile.Center, 0, 80, 80);
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (AITimer > 60)
            {
                Projectile.velocity += Projectile.Center.DirectionTo(Main.player[(int)Target].Center) * 1.5f;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity -= 1f / 30f;
            }
            else
            {
                if (Projectile.Opacity < 1)
                {
                    Projectile.Opacity += 1f / 30f;
                }
            }
            AITimer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {

        }
    }
}
