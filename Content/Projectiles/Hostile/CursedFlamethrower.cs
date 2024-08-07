using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class CursedFlamethrower : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 24;
            Projectile.frameCounter = 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.CursedInferno, 180);
        }

        public override void AI()
        {
            Projectile.velocity /= 1.05f;
            Lighting.AddLight(Projectile.Center, 0, 10, 0);
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch);
            dust.noGravity = true;
            AITimer++;
            Projectile.alpha += 10;
            Projectile.rotation = AITimer / Main.rand.Next(2, 4);
            Projectile.frameCounter--;
            if (Projectile.frameCounter == 0)
            {
                if (Projectile.frame < 2)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 2;
                }
                else if (Projectile.frame >= 2 && Projectile.frame < 6)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 6;
                }
                else
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 1;
                }
            }
        }
    }
}
