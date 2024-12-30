using Microsoft.Xna.Framework;
using Paracosm.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Player player = Main.player[projectile.owner];
            if (Main.myPlayer == projectile.owner && player.GetModPlayer<ParacosmPlayer>().secondHand)
            {
                if (projectile.CountsAsClass(DamageClass.Melee))
                {
                    projectile.position = projectile.Center;
                    projectile.scale *= 1.25f;
                    projectile.width = (int)(projectile.width * 1.25f);
                    projectile.height = (int)(projectile.height * 1.25f);
                    projectile.Center = projectile.position;
                }
            }
        }

        public override void PostAI(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            if (player.GetModPlayer<ParacosmPlayer>().commandersWill && projectile.sentry)
            {
                player.GetModPlayer<ParacosmPlayer>().sentryCount++;
                float angleDeg = player.GetModPlayer<ParacosmPlayer>().sentryCount * (360 / player.maxTurrets);
                Vector2 position = player.Center - (Vector2.UnitY * 200).RotatedBy(MathHelper.ToRadians(angleDeg));
                projectile.velocity = projectile.Center.DirectionTo(position) * projectile.Center.Distance(position) / 12;
                projectile.tileCollide = false;
                projectile.timeLeft -= 30;
            }
        }
    }
}
