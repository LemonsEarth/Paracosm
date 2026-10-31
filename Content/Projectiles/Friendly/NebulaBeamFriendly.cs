using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly;

public class NebulaBeamFriendly : ModProjectile
{
    float AITimer = 0;
    ref float BeamLength => ref Projectile.ai[0];
    Vector2 direction = Vector2.Zero;

    const int BeamWidth = 42;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.width = BeamWidth;
        Projectile.height = BeamWidth;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 90;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.DamageType = DamageClass.Melee;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.damage = (int)(Projectile.damage * 0.75f);
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (AITimer == 0)
        {
            Projectile.Opacity = 0;
            SoundEngine.PlaySound(SoundID.Zombie104 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest });

            // Projectile.velocity is initialized from the firing player's mouse direction.
            // Use the projectile owner here so remote clients do not use their own mouse.
        }

        Projectile.Center = player.Center;
        if (Main.myPlayer == Projectile.owner)
        {
            direction = player.DirectionTo(Main.MouseWorld);
            Projectile.rotation = direction.ToRotation();
        }

        if (AITimer % 5 == 0)
        {
            Projectile.netUpdate = true;
        }

        if (AITimer % 2 == 0)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HallowSpray);
        }

        Lighting.AddLight(Projectile.Center, 100, 20, 100);

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
        Vector2 end = Projectile.Center + direction * (Math.Max(BeamLength, 1f) * BeamWidth - BeamWidth / 2f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamWidth, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];
        int beamLength = Math.Max((int)BeamLength, 1);
        Vector2 origin = new(texture.Width / 2f, frameHeight / 2f);

        for (int i = 0; i < beamLength; i++)
        {
            int frame = i == 0 ? 0 : i == beamLength - 1 ? 2 : 1;
            Rectangle sourceRectangle = new(0, frame * frameHeight, texture.Width, frameHeight);
            Vector2 drawPosition = Projectile.Center + direction * i * BeamWidth - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Color.White * Projectile.Opacity, Projectile.rotation, origin, 1f, SpriteEffects.None);
        }

        return false;
    }
}
