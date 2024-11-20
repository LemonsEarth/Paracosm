using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class ScarletCloudMoving : ModProjectile
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
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 7200;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<ScarletCloud>())
                {
                    proj.Kill();
                }
            }
        }

        public override void AI()
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
            Projectile.velocity = (SpawnPos - Projectile.Center).SafeNormalize(Vector2.Zero) * 12;
            if (Projectile.Center.Distance(SpawnPos) < 10)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), SpawnPos, Vector2.Zero, ModContent.ProjectileType<ScarletCloud>(), Projectile.damage, 1f);
                    Projectile.Kill();
                }
            }
            AITimer++;
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 7)
                {
                    Projectile.frame = 0;
                }
            }
        }
    }
}
