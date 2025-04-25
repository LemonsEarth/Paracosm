using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class SuperVoidBolt : ModProjectile
    {
        int AITimer = 0;

        ref float Distance => ref Projectile.ai[0];
        ref float Speed => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 1;
            Projectile.Opacity = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.5f }, Projectile.Center);
            }

            float sinMovement = (float)Math.Sin(AITimer / Speed) * Distance;
            Vector2 normal = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
            Projectile.Center += normal * sinMovement;
            Projectile.rotation = Projectile.oldPos[1].DirectionTo(Projectile.position).ToRotation();
            //Projectile.Opacity = (float)Math.Sin(AITimer / 4) * 0.5f + 0.5f;
            if (AITimer > 5 && AITimer < 30)
            {
                Projectile.Opacity += 0.05f;
            }
            if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity = MathHelper.Lerp(0f, 1f, Projectile.timeLeft / 30f);
            }

            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<VoidExplosion>(), (int)(Projectile.originalDamage * 2), Projectile.knockBack, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                if (k % 4 == 0) continue;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(DrawOffsetX, 0);
                Color color = Projectile.GetAlpha(lightColor * 0.5f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos + drawOrigin, texture.Frame(1, 5, 0, Projectile.frame), color * Projectile.Opacity, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
