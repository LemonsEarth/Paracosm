﻿using System;
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

namespace Paracosm.Content.Projectiles
{
    public class ParacosmicFlame : ModProjectile
    {
        float AITimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 24;
            Projectile.frameCounter = 2;
            Projectile.penetrate = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 0 });
        }

        public override void AI()
        {
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
            if (AITimer % 2 == 0)
            {
                Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.IceTorch);
            }
        }
    }
}