using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class VoidExplosion : ModProjectile
    {
        int AITimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Generic;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Infected>(), 240);
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.rotation = MathHelper.ToRadians(Main.rand.NextFloat(0, 360));
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 10)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }
    }
}
