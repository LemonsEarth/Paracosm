using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class LightsEndBeam : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 23;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            DrawOriginOffsetY = 16;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item43, Projectile.Center);
            }
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 23)
                {
                    Projectile.frame = 0;
                }
            }
            var dust = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-170, 170)), Vector2.Zero, ModContent.ProjectileType<NightActualSlash>(), Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                }
            }
        }
    }
}
