using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class FireBloom : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        NPC closestEnemy;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;
            
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.friendly = false;
            Projectile.sentry = true;
        }
        float attackSpeed = 0;
        public override void AI()
        {
            closestEnemy = GetClosestNPC(1000);

            Projectile.velocity.Y = 10f;
            if (closestEnemy != null)
            {
                attackSpeed = Projectile.Center.Distance(closestEnemy.Center) / 10;
                if (attackSpeed < 10)
                {
                    attackSpeed = 10;
                }
            }
            if (Main.myPlayer == Projectile.owner && closestEnemy != null && AITimer >= attackSpeed)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Lava);
                }
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, -Vector2.UnitY.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-60, 60))) * 6, ProjectileID.BallofFire, Projectile.damage, 2f);
                AITimer = 0;
                Projectile.netUpdate = true;
            }

            int frameDur = 20;
            Projectile.frameCounter++;
            if (Projectile.frameCounter == frameDur)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame == 2)
                {
                    Projectile.frame = 0;
                }
            }
            if (closestEnemy != null)
            {
                AITimer++;
            }
            else
            {
                AITimer = 0;
            }
        }

        public NPC GetClosestNPC(int distance)
        {
            NPC closestEnemy = null;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy() && Projectile.Center.Distance(npc.Center) < distance)
                {
                    if (closestEnemy == null)
                    {
                        closestEnemy = npc;
                    }
                    float distanceToNPC = Projectile.Center.DistanceSQ(npc.Center);
                    if (distanceToNPC < Projectile.Center.DistanceSQ(closestEnemy.Center))
                    {
                        closestEnemy = npc;
                    }
                }
            }
            return closestEnemy;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = 0;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
    }
}
