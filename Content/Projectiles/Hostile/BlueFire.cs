using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class BlueFire : ModProjectile
    {
        int projectileFrame = 0;
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 480;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        Player closestPlayer = Main.player[0];
        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 0 });
            }
            AITimer++;
            if (AITimer % 10 == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            foreach (Player p in Main.player)
            {
                {
                    if (Projectile.position.Distance(p.position) < Projectile.position.Distance(closestPlayer.position))
                    {
                        closestPlayer = p;
                    }
                }
            }
            Projectile.velocity = (closestPlayer.position - Projectile.position).SafeNormalize(Vector2.Zero) * 5;

            if (AITimer % 5 == 0)
            {
                projectileFrame++;
                if (projectileFrame == 3)
                {
                    projectileFrame = 0;
                }
            }
            Projectile.frame = projectileFrame;
        }
    }
}
