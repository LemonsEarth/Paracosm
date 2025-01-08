using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VoidEruptionRetractable : ModProjectile
    {
        int AITimer = 0;
        ref float NPCToFollow => ref Projectile.ai[0];
        ref float MoveTime => ref Projectile.ai[1];
        ref float RetractTime => ref Projectile.ai[2];

        public override string Texture => "Paracosm/Content/Projectiles/Hostile/VoidEruption";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1800;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.frame = 2;
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item116 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center);
            }
            NPC nameless = Main.npc[(int)NPCToFollow];
            if (nameless is null || !nameless.active || nameless.life == 0)
            {
                Projectile.Kill();
            }

            Projectile.rotation = nameless.Center.DirectionTo(Projectile.Center).ToRotation();

            if (AITimer > MoveTime)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.velocity = (nameless.Center - Projectile.Center) / 12;
            }

            if (AITimer - MoveTime > RetractTime)
            {
                Projectile.Kill();
            }

            AITimer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            float nan = float.NaN;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.npc[(int)NPCToFollow].Center, Projectile.Center, 17, ref nan))
            {
                return true;
            }

            return false;
        }
    }
}
