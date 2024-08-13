using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Paracosm.Content.Projectiles;
using rail;

namespace Paracosm.Content.Projectiles.Minions
{
    public class ParacosmicEye : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        float attackTimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.frameCounter = 9;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (IsPlayerAlive(owner) == false)
            {
                return;
            }
            AITimer++;
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 9)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 5)
                {
                    Projectile.frame = 0;
                }
            }

            SearchForEnemies(owner, out bool hasTarget, out Vector2 targetCenter, out float DistanceToTarget);
            Movement(owner, hasTarget, targetCenter, DistanceToTarget);
        }

        bool IsPlayerAlive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<ParacosmicEyeBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<ParacosmicEyeBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }

        void SearchForEnemies(Player owner, out bool hasTarget, out Vector2 targetCenter, out float distanceToTarget)
        {
            distanceToTarget = 500f;
            targetCenter = Projectile.position;
            hasTarget = false;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                float distance = target.Center.Distance(Projectile.Center);

                if (distance < 1000)
                {
                    distanceToTarget = distance;
                    targetCenter = target.Center;
                    hasTarget = true;
                }
            }

            if (!hasTarget)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float distance = Projectile.Center.Distance(npc.Center);
                        bool closest = Projectile.Center.Distance(targetCenter) > distance;
                        bool isInRange = Projectile.Center.Distance(npc.Center) < distanceToTarget;

                        if ((closest && isInRange) || !hasTarget)
                        {
                            distanceToTarget = distance;
                            targetCenter = npc.Center;
                            hasTarget = true;
                        }
                    }
                }
            }
        }

        int posCounter = 0;
        Vector2 randomPos = Vector2.Zero;
        Vector2 dashDirection = Vector2.Zero;
        void Movement(Player owner, bool hasTarget, Vector2 targetCenter, float distanceToTarget)
        {
            Vector2[] cornerPositions =
            {
                new Vector2(-50, -50),
                new Vector2(50, -50),
                new Vector2(50, 50),
                new Vector2(-50, 50)
            };

            if (!hasTarget)
            {
                attackTimer = 0;
                Projectile.rotation = MathHelper.ToRadians(AITimer) * 3;
                Projectile.velocity = ((owner.Center + cornerPositions[posCounter]) - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.Center.Distance((owner.Center + cornerPositions[posCounter])) / Main.rand.Next(5, 11);
                if (AITimer % 30 == 0)
                {
                    posCounter++;
                    if (posCounter == 4) posCounter = 0;
                }
            }
            else
            {
                Projectile.rotation = MathHelper.ToRadians(AITimer) * 6;

                attackTimer++;
                if (attackTimer == 0)
                {
                    randomPos = targetCenter + cornerPositions[Main.rand.Next(0, 4)] * 3;
                }
                if (attackTimer < 15)
                {
                    if (randomPos != Vector2.Zero)
                    Projectile.velocity = (randomPos - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.Center.Distance(randomPos) / Main.rand.Next(15, 20);
                }
                else if (attackTimer == 15)
                {
                    dashDirection = (targetCenter - Projectile.Center).SafeNormalize(Vector2.Zero);
                }
                else if (attackTimer > 15 && attackTimer < 30)
                {
                    if (dashDirection != Vector2.Zero)
                    Projectile.velocity = dashDirection * 40;
                }
                else
                {
                    attackTimer = -1;
                }
            }
        }
    }
}
