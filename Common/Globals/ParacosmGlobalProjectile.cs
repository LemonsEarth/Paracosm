using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Buffs.Cooldowns;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

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
