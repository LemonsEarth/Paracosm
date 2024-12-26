using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class IceProjectile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        NPC closestEnemy;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (AITimer % 30 == 0)
            {
                closestEnemy = GetClosestNPC(500);
                if (closestEnemy != null)
                {
                    Projectile.velocity = Projectile.Center.DirectionTo(closestEnemy.Center) * 5;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemSapphire);
            dust.noGravity = true;
            Lighting.AddLight(Projectile.Center, 2, 2, 5);
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 5, DustID.GemSapphire);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 240);
        }
    }
}
