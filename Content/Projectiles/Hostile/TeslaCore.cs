using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class TeslaCore : ModProjectile
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
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.alpha = 255;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62);
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.MushroomTorch, 1.2f);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 16; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitY.RotatedBy(MathHelper.ToRadians(i * 22.5f + gunMod * 20)) * 3.5f, ModContent.ProjectileType<TeslaShot>(), Projectile.damage, Projectile.knockBack, ai1: 3f, ai2: 1);
                }
                Projectile.netUpdate = true;
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
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, AITimer / (60 + gunMod * 30));

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
