using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class DivineFlamethrower : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        Vector2 savedVelocity
        {
            get => new Vector2(Projectile.ai[1], Projectile.ai[2]);
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }
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
            Projectile.timeLeft = 48;
            Projectile.frameCounter = 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Ichor, 180);
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                savedVelocity = Projectile.velocity;
                Projectile.velocity = new Vector2(0.1f, 0.1f);
                Projectile.netUpdate = true;
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, savedVelocity, 0.1f);
            Lighting.AddLight(Projectile.Center, 10, 10, 0);
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.OrangeTorch, Scale: 2.5f);
            dust.noGravity = true;
            AITimer++;
            Projectile.alpha += 5;
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
