using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class WindSlash : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        bool released = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.alpha = 200;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 20 && Projectile.alpha > 150)
            {
                Projectile.alpha -= 25;
            }
            else
            {
                Projectile.alpha += 25;
            }
            AITimer++;
            Player player = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation();
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, -Projectile.velocity.X, -Projectile.velocity.Y);

            if (player.channel)
            {
                Projectile.velocity = new Vector2((float)Math.Sin(AITimer / 10) * 10, 0);
            }
            else
            {
                if (released == false && Projectile.owner == Main.myPlayer)
                {
                    Projectile.timeLeft = 180;
                    Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 20;
                    Projectile.netUpdate = true;
                }
                released = true;
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = (Projectile.oldPos[i] - Main.screenPosition) + drawOrigin + new Vector2(0, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(Texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
