using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class DivineSpiritFlame : ModProjectile
    {
        float AITimer = 0;
        ref float SpawnTime => ref Projectile.ai[0];
        ref float Speed => ref Projectile.ai[1];
        ref float PlayedIDToTrack => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            AIType = 0;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie53 with { MaxInstances = 8, PitchVariance = 1.0f, Volume = 0.2f });
            }
            if (AITimer < SpawnTime)
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 255 / (int)SpawnTime;
                }
            }
            else
            {
                Projectile.velocity = (Main.player[(int)PlayedIDToTrack].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Speed;
            }

            for (int i = 0; i < 2; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position - new Vector2(2f, 2f), Projectile.width + 2, Projectile.height + 2, DustID.OrangeTorch, Scale: 1.5f);
                dust.noGravity = true;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            AITimer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Ichor, 300);
        }
    }
}
