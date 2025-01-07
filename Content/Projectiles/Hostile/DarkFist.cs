using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class DarkFist : ModProjectile
    {
        int AITimer = 0;
        ref float SpeedMul => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            DrawOffsetX = -15;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                if (SpeedMul == 0)
                {
                    SpeedMul = 1;
                }
                SoundEngine.PlaySound(SoundID.Zombie81 with { MaxInstances = 2, PitchRange = (-0.5f, -0.3f)});
            }

            Projectile.velocity *= SpeedMul;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 3)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, Main.rand.NextFloat(15, 20), DustID.Granite, Main.rand.NextFloat(1.2f, 1.8f), true);
            SoundEngine.PlaySound(SoundID.Item89 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest, PitchRange = (-0.2f, 0.2f)}, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, k);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, DrawOriginOffsetY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                color.A /= 2;
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
