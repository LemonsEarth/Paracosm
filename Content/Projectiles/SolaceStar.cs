using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class SolaceStar : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float RandomRot => ref Projectile.ai[1];
        NPC closestEnemy;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 10;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 50;
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 12)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = MathHelper.ToRadians(AITimer * 10);
            AITimer++;
            if (AITimer % 20 == 0)
            {
                if (Projectile.owner == Main.myPlayer)
                {

                    for (int i = 0; i < 3; i++)
                    {
                        if (LemonUtils.GetClosestNPC(Projectile) == null)
                        {
                            return;
                        }
                        closestEnemy = LemonUtils.GetClosestNPC(Projectile);
                        RandomRot = Main.rand.Next(-15, 15);
                        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Projectile.Center.DirectionTo(closestEnemy.Center).RotatedBy(MathHelper.ToRadians(RandomRot)) * 16, ProjectileID.HallowStar, Projectile.damage, 1f); 
                        Projectile.netUpdate = true;
                    }
                }
            }
        }
    }
}
