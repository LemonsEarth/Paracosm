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

        public override string Texture => "Paracosm/Content/Projectiles/Hostile/VoidEruption";

        public Vector2 Position { get; set; }
        public float TimeToPosition { get; set; }
        public float RetractTime { get; set; }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(TimeToPosition);
            writer.Write(RetractTime);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            TimeToPosition = reader.ReadSingle();
            RetractTime = reader.ReadSingle();
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
                SoundEngine.PlaySound(SoundID.Item116 with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest });
            }
            NPC nameless = Main.npc[(int)NPCToFollow];
            if (nameless is null || !nameless.active || nameless.life == 0)
            {
                Projectile.Kill();
            }

            Projectile.rotation = nameless.Center.DirectionTo(Projectile.Center).ToRotation();

            if (AITimer < TimeToPosition)
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, Position, 1 / TimeToPosition);
            }
            else
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, nameless.Center, 1 / RetractTime);
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
