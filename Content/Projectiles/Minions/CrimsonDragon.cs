using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Minions
{
    public class CrimsonDragon : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float AttackTimer => ref Projectile.ai[1];
        ref float AttackCount => ref Projectile.ai[2];
        NPC closestNPC;
        Vector2 offset = new Vector2(75, -75);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.frameCounter = 0;
        }

        public override bool MinionContactDamage()
        {
            return false;
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

            Movement(owner);

            Attack();
            if (closestNPC != null)
            {
                Projectile.spriteDirection = -Math.Sign(closestNPC.Center.X - Projectile.Center.X);
            }

            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.OrangeTorch, Scale: 1f);
            dust.noGravity = true;
            AITimer++;
        }

        void Movement(Player player)
        {
            Projectile.velocity = Projectile.Center.DirectionTo(player.Center + (offset - new Vector2(0, Projectile.minionPos * 18))) * Projectile.Center.Distance(player.Center + (offset - new Vector2(0, Projectile.minionPos * 18))) / 10;
        }

        void Attack()
        {
            closestNPC = LemonUtils.GetClosestNPC(Projectile);
            if (closestNPC != null)
            {
                if (AttackTimer == 0)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(closestNPC.Center) * 12, ProjectileID.GoldenShowerFriendly, Projectile.damage, 1f);
                        Projectile.netUpdate = true;
                    }
                    AttackTimer = 30;
                    AttackCount++;
                    if (AttackCount == 3)
                    {
                        AttackCount = 0;
                        if (Main.myPlayer == Projectile.owner)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(closestNPC.Center).RotatedBy(i * MathHelper.PiOver4) * 12, ModContent.ProjectileType<BloodDrop>(), Projectile.damage, 1f);
                            }
                        }
                    }
                }

                AttackTimer--;
            }
        }

        bool IsPlayerAlive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CrimsonDragonBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<CrimsonDragonBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }
    }
}
