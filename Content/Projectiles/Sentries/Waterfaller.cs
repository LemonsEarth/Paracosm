using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class Waterfaller : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float AttackTimer => ref Projectile.ai[1];
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
            closestEnemy = GetClosestNPC(1000);

            Projectile.velocity.Y = 10f;
            if (closestEnemy != null)
            {
                if (AttackTimer == 60)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 offset = new Vector2(Main.rand.NextFloat(-40, 40), -500 + Main.rand.NextFloat(-40, 40));
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), closestEnemy.Center + offset, Vector2.UnitY * 2, ModContent.ProjectileType<WaterfallDrop>(), Projectile.damage, 1f);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item21 with { Volume = 0.5f, PitchRange = (-0.2f, 0.2f) }, Projectile.Center);
                    AttackTimer = 0;
                }
                AttackTimer++;
            }
            else
            {
                AttackTimer = 0;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 20)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            Lighting.AddLight(Projectile.Center, 0, 0, 5);
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
