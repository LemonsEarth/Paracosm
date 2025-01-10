using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class NebulaBeamFriendly : ModProjectile
    {
        float AITimer = 0;
        ref float SegmentCount => ref Projectile.ai[0];
        ref float SegmentFrame => ref Projectile.ai[1];
        Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage /= 2;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.Opacity = 0;
                SoundEngine.PlaySound(SoundID.Zombie104 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest });
                Projectile.rotation = Projectile.velocity.ToRotation();
                direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.frame = (int)SegmentFrame;
                if (Main.myPlayer == Projectile.owner)
                {
                    if (SegmentCount > 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * Projectile.height, direction, Type, Projectile.damage, 1, Projectile.owner, ai0: SegmentCount - 1, ai1: 1);
                    }
                    else if (SegmentCount > 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * Projectile.height, direction, Type, Projectile.damage, 1, Projectile.owner, ai0: SegmentCount - 1, ai1: 3);
                    }
                }
            }

            if (AITimer % 2 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HallowSpray);
            }

            Lighting.AddLight(Projectile.Center, 100, 20, 100);

            if (Projectile.timeLeft > 60)
            {
                Projectile.Opacity += 2f / 10f;
            }
            else if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity -= 1f / 30f;
            }

            Projectile.velocity = Vector2.Zero;

            AITimer++;
        }
    }
}
