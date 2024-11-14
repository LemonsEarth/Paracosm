using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class IndicatorLaser : ModProjectile
    {
        int AITimer = 0;
        ref float SourceID => ref Projectile.ai[0];
        Vector2 PosToDrawTo
        {
            get { return new Vector2(Projectile.ai[1], Projectile.ai[2]); }
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }
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
            }
            if (Projectile.timeLeft > 10)
            {
                if (Projectile.alpha > 0)
                Projectile.alpha -= 255 / 10;
            }
            else if (Projectile.timeLeft < 10)
            {
                Projectile.alpha += 255 / 10;
            }
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = PosToDrawTo;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, Projectile.height / 2);
            Vector2 posToProj = Projectile.Center - PosToDrawTo;
            int segmentHeight = 64;
            float rotation = Projectile.velocity.ToRotation();
            float distanceLeft = posToProj.Length() + segmentHeight / 2;

            while (distanceLeft > 0)
            {
                drawPosition += posToProj.SafeNormalize(Vector2.Zero) * segmentHeight;
                distanceLeft = drawPosition.Distance(Projectile.Center);
                distanceLeft -= segmentHeight;
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, null, Projectile.GetAlpha(lightColor), rotation, drawOrigin, 1f, SpriteEffects.None);
            }
            return true;
        }
    }
}
