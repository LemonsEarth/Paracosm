﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class ParaSwordShard : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        Vector2 tempMousePos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, Main.projFrames[Projectile.type]);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            AITimer++;
            if (AITimer % 3 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Corruption);
            }
            Projectile.rotation = AITimer / 2;
            if (Projectile.ai[1] == 1)
            {
                Projectile.DamageType = DamageClass.Magic;
                Projectile.tileCollide = false;
                if (AITimer == 45 && Projectile.owner == Main.myPlayer)
                {
                    tempMousePos = Main.MouseWorld;
                    Projectile.velocity = (tempMousePos - Projectile.Center).SafeNormalize(Vector2.Zero) * 20;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.tileCollide = true;
                Projectile.DamageType = DamageClass.Melee;
            }
        }
    }
}
