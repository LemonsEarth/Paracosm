using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Paracosm.Content.Projectiles.Friendly;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Minions
{
    public class TeslaGunMinion : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float AttackTimer => ref Projectile.ai[1];
        ref float NotAttackTimer => ref Projectile.ai[2];
        NPC closestNPC;
        Vector2 offset = new Vector2(40, -40);
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
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        const int BASE_ATTACK_CD = 10;

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
                if (Projectile.frame >= 5)
                {
                    Projectile.frame = 0;
                }
            }

            Movement(owner);

            if (AITimer == 0)
            {
                Projectile.rotation = owner.Center.DirectionTo(Projectile.Center).ToRotation();
            }
            if (AttackTimer <= 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    NPC target = LemonUtils.GetClosestNPC(Projectile);
                    if (target != null && target.active && target.CanBeChasedBy())
                    {
                        Projectile.rotation = Projectile.Center.DirectionTo(target.Center).ToRotation();
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(target.Center) * 15 * (1 + NotAttackTimer / 60f), ModContent.ProjectileType<TeslaShotFriendly>(), Projectile.damage * (int)(1 + NotAttackTimer / 60f), 0.8f);
                        NotAttackTimer = 0;
                        AttackTimer = BASE_ATTACK_CD;
                    }
                }
            }

            if (NotAttackTimer < 180)
            {
                NotAttackTimer++;
            }
            else
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFlare, 2, 2);
            }

            if (AttackTimer > 0)
            {
                AttackTimer -= 1f / (1 + (owner.velocity.Length() / 5));
            }
            AITimer++;
        }

        void Movement(Player player)
        {
            Projectile.Center = player.Center + offset.RotatedBy(Projectile.minionPos * MathHelper.PiOver2) * (1 + (int)Math.Ceiling(Projectile.minionPos / 4f));
        }

        bool IsPlayerAlive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<TeslaGunMinionBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<TeslaGunMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }
    }
}
