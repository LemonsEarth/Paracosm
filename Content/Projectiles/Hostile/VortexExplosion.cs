using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VortexExplosion : ModProjectile
    {
        int AITimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 24;
            Projectile.alpha = 0;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.VortexDebuff, 120);
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.rotation = MathHelper.ToRadians(Main.rand.NextFloat(0, 360));
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }
    }
}
