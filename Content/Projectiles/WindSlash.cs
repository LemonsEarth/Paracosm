using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Paracosm.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class WindSlash : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        bool released = false;
        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 20 && Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
            }
            else
            {
                Projectile.alpha += 25;
            }
            AITimer++;
            Player player = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation();
            Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.WhiteTorch, -Projectile.velocity.X, -Projectile.velocity.Y);

            if (player.channel)
            {
                Projectile.velocity = new Vector2((float)Math.Sin(AITimer / 10) * 10, 0);
            }
            else
            {
                if (released == false && Projectile.owner == Main.myPlayer)
                {
                    Projectile.timeLeft = 180;
                    Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 20;
                    Projectile.netUpdate = true;
                }
                released = true;
            }

        }
    }
}
