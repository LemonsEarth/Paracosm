using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class Shadowflower : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float AttackTimer => ref Projectile.ai[1];
        NPC closestEnemy;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 56;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.friendly = false;
            Projectile.sentry = true;
        }

        public override void AI()
        {
            closestEnemy = GetClosestNPC(1000);

            Projectile.velocity.Y = 10f;
            if (closestEnemy != null)
            {
                if (AttackTimer == 90)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -2; i < 3; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(closestEnemy.Center).RotatedBy(MathHelper.ToRadians(9) * i) * 10, ModContent.ProjectileType<ShadowBolt>(), Projectile.damage, 1f);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item43 with { Volume = 0.5f, PitchRange = (-0.2f, 0.2f) });
                    AttackTimer = 0;
                }
                AttackTimer++;
            }
            else
            {
                AttackTimer = 0;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            Lighting.AddLight(Projectile.Center, 5, 0, 5);
            AITimer++;
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
