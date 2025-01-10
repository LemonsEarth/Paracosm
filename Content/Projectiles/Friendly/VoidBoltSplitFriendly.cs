using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class VoidBoltSplitFriendly : ModProjectile
    {
        int AITimer = 0;
        ref float DeathTime => ref Projectile.ai[0];
        ref float SplitCount => ref Projectile.ai[1];
        float speed = 1;
        NPC closestNPC;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.Opacity = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            DrawOffsetX = -15;
        }

        public override void AI()
        {
            if (AITimer >= DeathTime)
            {
                Projectile.Kill();
            }

            if (SplitCount == 0)
            {
                if (speed < 60)
                {
                    speed += 1f;
                }
                closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    Projectile.velocity = Projectile.Center.DirectionTo(closestNPC.Center) * speed;
                }
                Projectile.penetrate = 1;
            }
            Projectile.frameCounter++;
            Projectile.Opacity = 0.4f;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            AITimer++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            switch (SplitCount)
            {
                case >= 2: return Color.Red * Projectile.Opacity;
                case 1: return Color.White * Projectile.Opacity;
                default: return Color.Black * Projectile.Opacity;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item110 with { Volume = 0.5f, PitchRange = (-0.3f, 0.3f) }, Projectile.Center);
            LemonUtils.DustCircle(Projectile.Center, 16, 2, DustID.Granite, 1f);

            if (SplitCount <= 0)
            {
                return;
            }
            SplitCount--;
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, direction.RotatedBy(i * -MathHelper.PiOver2) * 12, Type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 60, ai1: SplitCount);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DarkBleed>(), 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f + DrawOriginOffsetX, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, 1);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, DrawOriginOffsetY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
