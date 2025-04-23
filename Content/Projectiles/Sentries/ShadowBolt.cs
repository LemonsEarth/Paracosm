using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class ShadowBolt : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = MathHelper.ToRadians(AITimer);

            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y);
            dust.noGravity = true;
            Lighting.AddLight(Projectile.Center, 2, 0, 2);
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 5, DustID.GemAmethyst);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
        }
    }
}
