﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class IndicatorLaser : ModProjectile
    {
        int projectileFrame = 0;
        float AITimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 8;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.alpha = 150;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item1 with { MaxInstances = 1 });
        }
        public override void AI()
        {
            AITimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
    }
}