using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class EquinoxSun : ModProjectile
    {
        public override string Texture => "Paracosm/Assets/Textures/FX/Empty32Tex";
        static readonly string SunTexture = "Paracosm/Content/Projectiles/Friendly/EquinoxSun";
        int AITimer = 0;
        ref float PosX => ref Projectile.ai[1];
        ref float PosY => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void OnSpawn(IEntitySource source)
        {
            PosX = Main.rand.NextBool().ToDirectionInt();
            PosY = Main.rand.NextBool().ToDirectionInt();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.SetDummyItemTime(2);
            Projectile.timeLeft = 2;
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 255 / 60;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item92 with { MaxInstances = 2 }, Projectile.Center);
            }

            if (AITimer == 30)
            {
                Projectile.damage *= 2;
                Projectile.netUpdate = true;
            }

            if (player.channel)
            {
                float rotSpeedDeg = AITimer * 2;
                Projectile.rotation = MathHelper.ToRadians(rotSpeedDeg);
                float distance = 33.94f; // Moon distance
                Vector2 offset = new Vector2(PosX, PosY).SafeNormalize(Vector2.Zero);
                Projectile.Center = player.MountedCenter + (offset * distance * 2).RotatedBy(MathHelper.ToRadians(rotSpeedDeg));
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 mouseDir = player.Center.DirectionTo(Main.MouseWorld);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, mouseDir * 12, ModContent.ProjectileType<EquinoxSunProj>(), Projectile.damage, 4f, ai0: 30f);
                }
                Projectile.Kill();
            }

            Lighting.AddLight(Projectile.Center, 10, 10, 2);
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
            dust.noGravity = true;

            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item90 with { PitchRange = (0.2f, 0.5f) }, Projectile.Center);
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.GemTopaz, 2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(SunTexture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
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
            shader.Shader.Parameters["color"].SetValue((Color.Yellow with { A = 10 }).ToVector4());
            shader.Apply();
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
