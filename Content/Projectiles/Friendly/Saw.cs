﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class Saw : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 5;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 50;
            }
            if (AITimer == 0)
            {
                var num = Main.rand.Next(0, 2);
                if (num == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.Item23, Projectile.Center);
                }
            }
            AITimer++;
            if (AITimer % 10 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
            }
            Projectile.rotation = AITimer;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + drawOrigin;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(Texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
