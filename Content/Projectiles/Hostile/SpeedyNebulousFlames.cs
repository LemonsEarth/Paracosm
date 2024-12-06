using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class SpeedyNebulousFlames : ModProjectile
    {
        int AITimer = 0;
        ref float SpawnTime => ref Projectile.ai[0];
        ref float Speed => ref Projectile.ai[1];
        bool DoIndicators
        {
            get { return Projectile.ai[2] >= 1 ? true : false; }
            set
            {
                Projectile.ai[2] = value == true ? 1 : 0;
            }
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 2 });
                if (DoIndicators)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 indicatorDistancePos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 64 * 50;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) / 10, ModContent.ProjectileType<IndicatorLaser>(), 10, 0, ai1: indicatorDistancePos.X, ai2: indicatorDistancePos.Y);
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
            }

            if (AITimer == SpawnTime)
            {
                Projectile.velocity *= Speed;
                Projectile.netUpdate = true;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 10)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            AITimer++;
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
