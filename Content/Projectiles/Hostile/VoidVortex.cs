using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VoidVortex : ModProjectile
    {
        int AITimer = 0;
        ref float DeathTime => ref Projectile.ai[0];
        ref float ShootInterval => ref Projectile.ai[1];
        ref float TurnInterval => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
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
                SoundEngine.PlaySound(SoundID.Item84 with { MaxInstances = 2, PitchRange = (-1f, -0.5f) });
            }

            if (ShootInterval > 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (AITimer % ShootInterval == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Vector2.UnitY * 10).RotatedBy(i * MathHelper.PiOver4), ModContent.ProjectileType<VoidBoltSplit>(), Projectile.damage / 2, 1f, ai0: 120, ai1: 0);
                        }
                    }
                }
            }

            if (TurnInterval > 0)
            {
                if (AITimer % TurnInterval == 0 && AITimer > 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.Pi * 2) * 0.75f;
                    }
                }
            }

            if (AITimer >= DeathTime)
            {
                Projectile.Kill();
            }

            foreach (Player player in Main.player)
            {
                if (player.Center.Distance(Projectile.Center) < 600)
                {
                    player.velocity += player.Center.DirectionTo(Projectile.Center) * 1f;
                }
            }

            Lighting.AddLight(Projectile.Center, 10, 80, 10);
            Projectile.rotation = MathHelper.ToRadians(AITimer * 6);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 3)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 8; i++)
                {
                    LemonUtils.DustCircle(Projectile.Center, 8, 2f * i, DustID.Granite, 1.3f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Vector2.UnitY * 20).RotatedBy(i * MathHelper.PiOver4), ModContent.ProjectileType<VoidBoltSplit>(), Projectile.damage / 2, 1f, ai0: 120, ai1: 0);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.Black;
                if (k == 0)
                {
                    color = Projectile.GetAlpha(lightColor);
                }
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, 1.5f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
