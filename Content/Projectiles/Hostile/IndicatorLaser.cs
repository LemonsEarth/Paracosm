using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class IndicatorLaser : ModProjectile
    {
        int AITimer = 0;
        ref float SegmentCount => ref Projectile.ai[0];
        Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { MaxInstances = 1 });
                Projectile.rotation = Projectile.velocity.ToRotation();
                direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (SegmentCount > 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * Projectile.height, direction, Type, Projectile.damage, 1, ai0: SegmentCount - 1);
                    }
                }
            }

            if (Projectile.timeLeft > 10)
            {
                Projectile.alpha = (int)MathHelper.Lerp(Projectile.alpha, 0, AITimer / 20f);
            }
            else if (Projectile.timeLeft < 10)
            {
                Projectile.alpha = (int)MathHelper.Lerp(Projectile.alpha, 255, 1f / Projectile.timeLeft);
            }
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, Projectile.height / 2);
            Color color = new(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
            Main.EntitySpriteDraw(texture, (Projectile.position + drawOrigin) - Main.screenPosition, null, color, Projectile.rotation, drawOrigin, 1f, SpriteEffects.None);
            return false;
        }
    }
}
