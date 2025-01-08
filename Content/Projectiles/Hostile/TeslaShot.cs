using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class TeslaShot : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float Acceleration => ref Projectile.ai[1];
        bool DoIndicators
        {
            get { return Projectile.ai[2] >= 1 ? true : false; }
            set
            {
                Projectile.ai[2] = value == true ? 1 : 0;
            }
        }
        float accelInit = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 15;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item94 with { PitchRange = (0.2f, 0.4f) }, Projectile.Center);
                accelInit = Acceleration;
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex);

            Lighting.AddLight(Projectile.Center, 0, 80, 80);
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Acceleration > 0)
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.velocity = direction * Acceleration;
                Acceleration += accelInit;
                if (AITimer == 0)
                {
                    if (DoIndicators)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, direction, ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 13);
                        }
                    }
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 15)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, k);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                color.A /= 2;
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
