using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class NebulousAuraHostile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float NPCtoFollow => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2000;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Projectile.alpha > 150)
            {
                Projectile.alpha -= 255 / 60;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item84 with { MaxInstances = 2, PitchRange = (-0.5f, 0f) }, Projectile.Center);
            }

            if (NPCtoFollow != -1)
            {
                if (Main.npc[(int)NPCtoFollow] == null || !Main.npc[(int)NPCtoFollow].active)
                {
                    Projectile.Kill();
                }
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Main.npc[(int)NPCtoFollow].Center;
            }

            Lighting.AddLight(Projectile.Center, 100, 80, 100);
            Projectile.rotation = AITimer;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 5)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, 1);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor);
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, 1.5f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
