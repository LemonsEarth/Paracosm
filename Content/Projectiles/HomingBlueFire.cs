﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class HomingBlueFire : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        NPC closestNPC;
        float speed = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;

        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 0 });
            }
            AITimer++;
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (AITimer >= 30)
            {
                speed++;
                closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    Projectile.velocity = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
                }
            }
        }
    }
}
