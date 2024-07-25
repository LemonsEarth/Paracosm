using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Sentries
{
    public class PoisonBloom : ModProjectile
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
            Projectile.aiStyle = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.friendly = true;
            Projectile.sentry = true;
        }

        public override void AI()
        {
            closestEnemy = GetClosestNPC();

            Projectile.velocity.Y = 2f;
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 60 == 0 && closestEnemy != null)
            {
                Vector2 spawnPos = Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(0, 20));
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, (closestEnemy.Center - spawnPos).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<PoisonBloomPetal>(), Projectile.damage, 2f);
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
            AITimer++;
        }

        public NPC GetClosestNPC()
        {
            NPC closestEnemy = null;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    if (closestEnemy == null)
                    {
                        closestEnemy = npc;
                    }
                    float distanceToNPC = Vector2.DistanceSquared(Projectile.Center, npc.Center);
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
    }
}
