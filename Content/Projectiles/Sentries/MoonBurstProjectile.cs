using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class MoonBurstProjectile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float reps => ref Projectile.ai[1];
        ref float slowDown => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 45;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item28);
            }
            Projectile.rotation = MathHelper.ToRadians(AITimer * 2);
            AITimer++;

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Projectile.velocity.X, Projectile.velocity.Y);
            Projectile.velocity /= slowDown;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (reps > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(1, 1).RotatedBy(i * MathHelper.PiOver4) * 10, Projectile.type, Projectile.damage / 2, Projectile.knockBack, ai1: reps - 1, ai2: 1.2f);
                        reps--;
                    }
                }
            }
        }
    }
}
