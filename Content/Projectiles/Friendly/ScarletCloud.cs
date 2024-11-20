using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class ScarletCloud : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        Vector2 SpawnPos
        {
            get => new Vector2(Projectile.ai[1], Projectile.ai[2]);
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 44;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 7200;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 15;
            }
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            Projectile.velocity = Vector2.Zero;

            if (AITimer % 5 == 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    SpawnPos = Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(10, Projectile.height - 10));
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), SpawnPos, new Vector2(0, 10), ProjectileID.BloodRain, Projectile.damage, 1f);
                }
            }
            if (AITimer % 30 == 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    SpawnPos = Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(10, Projectile.height - 10));
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), SpawnPos, new Vector2(0, 5), ModContent.ProjectileType<BloodDrop>(), Projectile.damage * 2, 1f);
                }
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }
    }
}
