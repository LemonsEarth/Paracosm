using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Bosses.TheNameless;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Projectiles.Hostile;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.HeldProjectiles
{
    public class JudgementProj : ModProjectile
    {
        int AITimer = 0;
        bool released = false;
        float holdTimer = 0;
        Vector2 mouseDir = Vector2.Zero;
        bool hitSomething = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 66;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.light = 1f;
            DrawOffsetX = -75;
            DrawOriginOffsetX = 30;
        }

        public override void OnSpawn(IEntitySource source)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 2, DustID.Granite);
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }

            Player player = Main.player[Projectile.owner];

            if (player is null || !player.active || player.statLife == 0)
            {
                Projectile.Kill();
                return;
            }

            if (player.channel && !released)
            {
                player.SetDummyItemTime(60);
                Projectile.Center = player.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 180;
                if (Main.myPlayer == player.whoAmI)
                {
                    mouseDir = player.Center.DirectionTo(Main.MouseWorld);
                    if (mouseDir.X >= 0)
                    {
                        player.ChangeDir(1);
                    }
                    else
                    {
                        player.ChangeDir(-1);
                    }
                }
                Projectile.rotation = mouseDir.ToRotation();
                if (holdTimer < 100)
                {
                    holdTimer++;
                }
                if (holdTimer == 100)
                {
                    LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.Granite);
                    if (Main.myPlayer == player.whoAmI)
                    {
                        LemonUtils.DustCircle(Main.MouseWorld, 16, 10, DustID.Granite);
                    }
                }
            }

            if (!player.channel)
            {
                released = true;
            }

            if (released)
            {
                float boostValue = (holdTimer / 10f) * 2;
                Projectile.velocity = mouseDir * (20 + boostValue);
                Projectile.rotation = Projectile.velocity.ToRotation();
                int splitInterval = 30 - (int)boostValue;
                if (AITimer % splitInterval == 0 && hitSomething)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            int splitCount = holdTimer > 80 ? 2 : 1;
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.PiOver2) * 7, ModContent.ProjectileType<VoidBoltSplitFriendly>(), Projectile.damage, 1f, Projectile.owner, ai0: 30, ai1: splitCount);
                        }
                    }       
                }
            }


            AITimer++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitSomething = true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
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
            if (!released) return true;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f + DrawOriginOffsetX, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, DrawOriginOffsetY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
