using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile;

public class VortexLaser : ModProjectile
{
    float AITimer = 0;
    ref float SegmentCount => ref Projectile.ai[0];
    Vector2 direction = Vector2.Zero;

    const int BeamWidth = 50;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.width = BeamWidth;
        Projectile.height = BeamWidth;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 120;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        if (AITimer == 0)
        {
            Projectile.Opacity = 0;
            SoundEngine.PlaySound(SoundID.Zombie104 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest });
            Projectile.rotation = Projectile.velocity.ToRotation();
            direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
        }

        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HallowSpray);

        if (Projectile.timeLeft > 60)
        {
            Projectile.Opacity += 2f / 10f;
        }
        else if (Projectile.timeLeft < 30)
        {
            Projectile.Opacity -= 1f / 30f;
        }

        Projectile.velocity = Vector2.Zero;

        AITimer++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 start = Projectile.Center - direction * (BeamWidth / 2f);
        Vector2 end = Projectile.Center + direction * (Math.Max(SegmentCount + 1f, 1f) * BeamWidth - BeamWidth / 2f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamWidth, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];
        int beamLength = Math.Max((int)SegmentCount + 1, 1);
        Vector2 origin = new(texture.Width / 2f, frameHeight / 2f);

        for (int i = 0; i < beamLength; i++)
        {
            int frame = i == 0 ? 0 : i == beamLength - 1 ? 3 : ((int)AITimer / 5 + i) % 2 + 1;
            Rectangle sourceRectangle = new(0, frame * frameHeight, texture.Width, frameHeight);
            Vector2 drawPosition = Projectile.Center + direction * i * BeamWidth - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, lightColor * Projectile.Opacity, Projectile.rotation, origin, 1f, SpriteEffects.None);
        }

        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.VortexDebuff, 120);
    }
}
