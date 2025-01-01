using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VoidBoltHostile : ModProjectile
    {
        int AITimer = 0;
        ref float WaitTime => ref Projectile.ai[0];
        ref float Speed => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.alpha = 255;
            DrawOffsetX = -30;
            DrawOriginOffsetX = 16;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap);
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { PitchRange = (-0.4f, -0.2f) });
                SoundEngine.PlaySound(SoundID.Item94 with { PitchRange = (0.2f, 0.4f) });
            }

            Lighting.AddLight(Projectile.Center, 10, 10, 10);
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (AITimer >= WaitTime)
            {
                Projectile.velocity.Y += Speed;
                //Projectile.velocity = Utils.AngleLerp(Projectile.velocity.SafeNormalize(Vector2.Zero).ToRotation(), -Vector2.UnitY.ToRotation(), (AITimer - WaitTime) / TimeToRotate).ToRotationVector2() * Speed;
            }         

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 3)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f + DrawOriginOffsetX, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, 1);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, DrawOriginOffsetY);
                Color color = Color.Black * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
