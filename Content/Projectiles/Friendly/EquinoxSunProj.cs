﻿using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class EquinoxSunProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float speed => ref Projectile.ai[1];
        NPC closestNPC;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (AITimer <= 0)
            {
                speed++;
                closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    Projectile.velocity = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
                }
            }

            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
            dust.noGravity = true;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 20)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer--;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Volume = 0.5f, PitchRange = (-0.3f, 0.3f) }, Projectile.position);
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.GemTopaz, 1.2f);
        }
    }
}
