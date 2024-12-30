using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class SpiritProjHeal : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float speed => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (AITimer <= 0)
            {
                speed++;
                Projectile.Opacity += 0.1f;
                Player owner = Main.player[Projectile.owner];
                if (owner.active && !owner.dead)
                {
                    Projectile.velocity = Projectile.Center.DirectionTo(owner.Center) * speed;
                    if (Projectile.Colliding(Projectile.Hitbox, owner.Hitbox))
                    {
                        owner.Heal(5);
                        Projectile.Kill();
                    }
                }
            }
            else
            {
                Projectile.Opacity = 0f;
                Projectile.velocity /= 1.1f;
            }

            for (int i = 0; i < 3; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.SpectreStaff);
                dust.noGravity = true;
            }

            int frameDur = 6;
            Projectile.frameCounter++;
            if (Projectile.frameCounter == frameDur)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            Lighting.AddLight(Projectile.Center, 1, 1, 1);

            AITimer--;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit36 with { PitchRange = (-0.3f, 0.3f), Volume = 0.5f }, Projectile.position);
            LemonUtils.DustCircle(Projectile.Center, 16, 5, DustID.GemDiamond);
        }
    }
}
