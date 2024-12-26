using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class WaterfallDrop : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 32;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 8;
            }
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            var dust = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.BlueTorch);
            Projectile.velocity *= 1.02f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            Lighting.AddLight(Projectile.Center, 0, 0, 1);
            AITimer++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFromList(-MathHelper.PiOver4, MathHelper.PiOver4));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = -oldVelocity / 2;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 5, DustID.GemSapphire);
        }
    }
}
