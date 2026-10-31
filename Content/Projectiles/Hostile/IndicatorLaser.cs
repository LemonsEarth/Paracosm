using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile;

public class IndicatorLaser : ModProjectile
{
    int AITimer = 0;
    ref float SegmentCount => ref Projectile.ai[0];
    Vector2 direction = Vector2.Zero;

    const int BeamWidth = 256;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2500;
    }

    public override void SetDefaults()
    {
        Projectile.width = BeamWidth;
        Projectile.height = BeamWidth;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 30;
        Projectile.alpha = 255;
    }
    public override void AI()
    {
        Projectile.damage = 0;
        if (AITimer == 0)
        {
            SoundEngine.PlaySound(SoundID.Item1 with { MaxInstances = 1 }, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();
            direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Projectile.Center += direction * BeamWidth / 2;
        }

        if (Projectile.timeLeft > 10)
        {
            Projectile.alpha = (int)MathHelper.Lerp(Projectile.alpha, 0, AITimer / 20f);
        }
        else if (Projectile.timeLeft < 10)
        {
            Projectile.alpha = (int)MathHelper.Lerp(Projectile.alpha, 255, 1f / Projectile.timeLeft);
        }
        AITimer++;
        Projectile.velocity = Vector2.Zero;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawOrigin = new(texture.Width / 2f, Projectile.height / 2f);
        Color color = new(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
        int beamLength = Math.Max((int)SegmentCount + 1, 1);

        for (int i = 0; i < beamLength; i++)
        {
            Vector2 drawPosition = Projectile.Center + direction * i * BeamWidth - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPosition, null, color, Projectile.rotation, drawOrigin, 1f, SpriteEffects.None);
        }

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 start = Projectile.Center - direction * (BeamWidth / 2f);
        Vector2 end = Projectile.Center + direction * (Math.Max(SegmentCount + 1f, 1f) * BeamWidth - BeamWidth / 2f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamWidth, ref collisionPoint);
    }
}
