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

namespace Paracosm.Content.Projectiles
{
    public class ParaSwordShard : ModProjectile
    {
        float AITimer = 0;
        Vector2 tempMousePos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, 4);
        }

        public override void AI()
        {
            AITimer++;
            Projectile.rotation = AITimer / 2;
            if (Projectile.ai[1] == 1)
            {
                Projectile.DamageType = DamageClass.Magic;
                Projectile.tileCollide = false;
                if (AITimer == 45)
                {
                    tempMousePos = Main.MouseWorld;
                    Projectile.velocity = Vector2.Normalize(tempMousePos - Projectile.Center) * 20;
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
