using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class Starshot : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float speed => ref Projectile.ai[1];
        NPC closestNPC;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 2;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer <= 0)
            {
                speed += 0.1f;
                closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    Projectile.velocity += Projectile.Center.DirectionTo(closestNPC.Center) * speed;
                }
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
            AITimer--;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, 0);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                color.A /= 2;
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
