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
    public class PoisonPetal : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (AITimer > 30)
            {
                Projectile.velocity.Y += 0.1f;
            }
            AITimer++;
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, Projectile.velocity.X, Projectile.velocity.Y);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.5f }, Projectile.Center);
            return true;
        }
    }
}
