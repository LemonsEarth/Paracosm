using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class StardustTailProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.RangeMultiplier = 1f;
            Projectile.WhipSettings.Segments = 30;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<StardustTailDebuff>(), 120);
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }

        public override void PostAI()
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz);
            dust.noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> points = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, points);
            Main.DrawWhip_WhipBland(Projectile, points);
            return false;
        }
    }
}
