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
    public class VortexLaser : ModProjectile
    {
        float AITimer = 0;
        ref float SegmentCount => ref Projectile.ai[0];
        ref float SegmentFrame => ref Projectile.ai[1];
        Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
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
                SoundEngine.PlaySound(SoundID.Zombie104);
                Projectile.rotation = Projectile.velocity.ToRotation();
                direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.frame = (int)SegmentFrame;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (SegmentCount > 1)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * Projectile.height, direction, Type, Projectile.damage, 1, ai0: SegmentCount - 1, ai1: 1);
                    }
                    else if (SegmentCount > 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + direction * Projectile.height, direction, Type, Projectile.damage, 1, ai0: SegmentCount - 1, ai1: 3);
                    }
                }
            }

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HallowSpray);

            if (Projectile.timeLeft > 60)
            {
                Projectile.Opacity += 2f/10f;
            }
            else if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity -= 1f / 30f;
            }

            if (SegmentFrame == 1 || SegmentFrame == 2)
            {
                if (AITimer % 5 == 0)
                {
                    Projectile.frame = Main.rand.Next(1, 3);
                }
            }

            Projectile.velocity = Vector2.Zero;

            AITimer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.VortexDebuff, 120);
        }
    }
}
