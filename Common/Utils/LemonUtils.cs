using Microsoft.Xna.Framework;
using Terraria;

namespace Paracosm.Common.Utils
{
    public static class LemonUtils
    {
        /// <summary>
        /// <para>Creates a circle of dust around a given position.</para>
        /// <para><paramref name="noGrav"/> - if false, dust will be affected by gravity.</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="amount"></param>
        /// <param name="speed"></param>
        /// <param name="dustID"></param>
        /// <param name="scale"></param>
        /// <param name="noGrav"></param>
        /// <param name="alpha"></param>
        /// <param name="newColor"></param>
        public static void DustCircle(Vector2 position, int amount, float speed, int dustID, float scale = 1, bool noGrav = true, int alpha = 0, Color newColor = default)
        {
            for (int i = 0; i < amount; i++)
            {
                var dust = Dust.NewDustDirect(position, 1, 1, dustID, Scale: scale);
                dust.velocity = new Vector2(0, -speed).RotatedBy(MathHelper.ToRadians(i * (360 / amount)));
                if (noGrav)
                {
                    dust.noGravity = true;
                }

            }
        }
        public static NPC GetClosestNPC(Projectile projectile)
        {
            NPC closestEnemy = null;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    if (closestEnemy == null)
                    {
                        closestEnemy = npc;
                    }
                    float distanceToNPC = Vector2.DistanceSquared(projectile.Center, npc.Center);
                    if (distanceToNPC < projectile.Center.DistanceSQ(closestEnemy.Center))
                    {
                        closestEnemy = npc;
                    }
                }
            }
            return closestEnemy;
        }

        public static Vector2 ApproxPos(Vector2 pos, float errorX, float errorY)
        {
            return pos + new Vector2(Main.rand.NextFloat(-errorX, errorX), Main.rand.NextFloat(-errorX, errorX));
        }
    }
}
