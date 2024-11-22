using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VortexMine : ModProjectile
    {
        float AITimer = 0;
        Vector2 targetPos
        {
            get { return new Vector2(Projectile.ai[0], Projectile.ai[1]); }
            set
            {
                Projectile.ai[0] = value.X;
                Projectile.ai[1] = value.Y;
            }
        }
        ref float gunMod => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 90;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.5f });
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.MushroomTorch, 1.2f);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<VortexExplosion>(), Projectile.damage * 2, Projectile.knockBack);
            }
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.timeLeft += (int)gunMod * 30;
            }
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, AITimer / 90);
            Projectile.rotation = MathHelper.ToRadians(AITimer * Projectile.velocity.Length());

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
    }
}
