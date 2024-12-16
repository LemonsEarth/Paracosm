using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class StarshotHostile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        bool DoIndicators
        {
            get { return Projectile.ai[1] >= 1 ? true : false; }
            set
            {
                Projectile.ai[1] = value == true ? 1 : 0;
            }
        }

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
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.penetrate = 2;
            Projectile.alpha = 255;
            Projectile.light = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            /*LemonUtils.DustCircle(Projectile.Center, 16, 2, DustID.BlueTorch, 1.2f);
            LemonUtils.DustCircle(Projectile.Center, 16, 2, DustID.YellowTorch);*/
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
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
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<IndicatorLaser>(), 0, 1, ai0: 5);
                    }
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
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                /*Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;*/
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
                var shaderdata = GameShaders.Misc["ProjectileLightShader"];
                shaderdata.UseImage0(TextureAssets.Projectile[Type]);
                shaderdata.Apply(new DrawData(texture, drawPos, drawRectangle, Color.White));
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
    }
}
