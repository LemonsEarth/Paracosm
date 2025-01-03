using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class DarkShineProj : ModProjectile
    {
        public override string Texture => "Paracosm/Assets/Textures/FX/Empty100Tex";
        int AITimer = 0;
        ref float TimeUntilShoot => ref Projectile.ai[0];
        Vector2 originalVelocity = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            Projectile.light = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 8, DustID.Granite, 2f);
            originalVelocity = Projectile.velocity;
            Projectile.velocity = Vector2.Zero;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (AITimer < TimeUntilShoot)
            {
                Projectile.velocity = -originalVelocity.SafeNormalize(Vector2.Zero);
            }
            else
            {
                Projectile.velocity += originalVelocity.SafeNormalize(Vector2.Zero) * 2;
            }
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Granite);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Paracosm/Assets/Textures/FX/Empty100Tex").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            var shader = GameShaders.Misc["Paracosm:ProjectileLightShader"];
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shader.Shader, Main.GameViewMatrix.TransformationMatrix);
            shader.Shader.Parameters["time"].SetValue(AITimer / 60f);
            shader.Shader.Parameters["color"].SetValue(Color.Black.ToVector4());
            shader.Shader.Parameters["shineRate"].SetValue(8);
            shader.Apply();
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
