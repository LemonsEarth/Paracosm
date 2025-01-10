using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class VoidVortexFriendly : ModProjectile
    {
        int AITimer = 0;

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
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item84 with { MaxInstances = 2, PitchRange = (-1f, -0.5f) }, Projectile.Center);
                Projectile.scale = 0.1f;
                Projectile.Opacity = 0f;
            }

            if (AITimer < 30)
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 2f, 1 / 30f);
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 1 / 30f);
            }

            if (Main.myPlayer == Projectile.owner)
            {
                MoveToPos(Main.MouseWorld, 1, 1, 2f, 2f);
            }

            if (AITimer % 60 == 0)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.Distance(Projectile.Center) < 1000 && !npc.boss && npc.CanBeChasedBy())
                    {
                        npc.velocity += npc.Center.DirectionTo(Projectile.Center) * 20;
                    }
                }
            }       

            if (AITimer % 5 == 0)
            {
                Projectile.netUpdate = true;
            }

            if (Projectile.timeLeft < 30)
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.05f, 1 / 30f);
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 1 / 30f);
            }

            Lighting.AddLight(Projectile.Center, 5, 8, 5);
            Projectile.rotation = MathHelper.ToRadians(AITimer * 12);

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

        void MoveToPos(Vector2 pos, float xAccel = 1f, float yAccel = 1f, float xSpeed = 1f, float ySpeed = 1f)
        {
            Vector2 direction = Projectile.Center.DirectionTo(pos);
            if (direction.HasNaNs())
            {
                return;
            }
            float XaccelMod = Math.Sign(direction.X) - Math.Sign(Projectile.velocity.X);
            float YaccelMod = Math.Sign(direction.Y) - Math.Sign(Projectile.velocity.Y);
            Projectile.velocity += new Vector2(XaccelMod * xAccel + xSpeed * Math.Sign(direction.X), YaccelMod * yAccel + ySpeed * Math.Sign(direction.Y));
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 8; i++)
                {
                    LemonUtils.DustCircle(Projectile.Center, 8, 2f * i, DustID.Granite, 1.3f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitY.RotatedBy(MathHelper.PiOver4 * i) * 6, ModContent.ProjectileType<VoidBoltSplitFriendly>(), Projectile.damage, 1f, Projectile.owner, 30, 1);
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
                    color = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
                }
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
