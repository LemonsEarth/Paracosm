using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class StardustEnergyMine : ModProjectile
    {
        float AITimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.5f });
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.MushroomTorch, 1.2f);
            for (int i = 0; i < 9; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(0.8f, 1.2f));
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<StardustExplosion>(), Projectile.damage * 2, Projectile.knockBack);
            }
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie67 with { Volume = 0.3f, PitchRange = (0.5f, 1f) });
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, AITimer / 180);
            Projectile.rotation = MathHelper.ToRadians(AITimer * Projectile.velocity.Length());
            Projectile.scale = (float)Math.Sin(MathHelper.ToRadians(AITimer * 4)) / 5 + 1; // 1/5 * sin(4x) + 1 ranges from 0.8 to 1.2

            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            var shader = GameShaders.Misc["Paracosm:ProjectileLightShader"];
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shader.Shader, Main.GameViewMatrix.TransformationMatrix);
            shader.Shader.Parameters["time"].SetValue(AITimer / 60f);
            shader.Apply();
            Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition + new Vector2(Projectile.width, Projectile.height) / 2, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
