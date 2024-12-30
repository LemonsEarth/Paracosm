using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class MoonBurst : ModProjectile
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

        public override void AI()
        {
            closestEnemy = GetClosestNPC(800);

            Projectile.velocity.Y = 10f;
            if (Main.myPlayer == Projectile.owner && closestEnemy != null && AITimer >= 120)
            {
                Vector2 spawnPos = Projectile.Center - new Vector2(0, 24);

                Dust.NewDust(spawnPos, 0, 0, DustID.IceTorch);
                AITimer = 0;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, (closestEnemy.Center - spawnPos).SafeNormalize(Vector2.Zero) * 8, ModContent.ProjectileType<MoonBurstProjectile>(), Projectile.damage, 2f, ai1: 2, ai2: 1);
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
            Lighting.AddLight(Projectile.Center, 0, 5, 5);
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
