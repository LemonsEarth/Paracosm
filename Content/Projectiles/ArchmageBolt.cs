using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class ArchmageBolt : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.LostSoulFriendly);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            AIType = ProjectileID.LostSoulFriendly;
            Projectile.aiStyle = ProjAIStyleID.LostSoul;
            Projectile.timeLeft = 120;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), ProjectileID.InfernoFriendlyBolt, Projectile.damage, Projectile.knockBack);
                }
                for (int i = 0; i < 4; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)), ProjectileID.ShadowBeamFriendly, Projectile.damage, Projectile.knockBack);
                }
            }
        }
    }
}
