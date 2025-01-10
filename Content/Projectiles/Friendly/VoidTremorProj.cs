using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Paracosm.Content.Projectiles.Hostile;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class VoidTremorProj : ModProjectile
    {
        int AITimer = 0;
        ref float MoveTime => ref Projectile.ai[0];
        ref float RetractTime => ref Projectile.ai[1];
        ref float RandAngle => ref Projectile.ai[2];
        float rotTimer = 0;

        public override string Texture => "Paracosm/Content/Projectiles/Hostile/VoidEruption";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1800;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(Main.rand.Next(-target.width, target.width), Main.rand.Next(-target.height, target.height));
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center + offset, Vector2.Zero, ModContent.ProjectileType<CraterExplosion>(), Projectile.damage, 1f, Projectile.owner);
                }
            }
        }

        public override void AI()
        {
            Projectile.frame = 2;
            Player player = Main.player[Projectile.owner];
            if (player is null || !player.active || player.statLife == 0)
            {
                Projectile.Kill();
            }

            if (AITimer == 0)
            {
                RandAngle = Main.rand.Next(-60, 60);
            }

            Projectile.rotation = player.Center.DirectionTo(Projectile.Center).ToRotation();

            rotTimer = Utils.AngleLerp(rotTimer, RandAngle, 1 / RetractTime);
            if (AITimer > MoveTime)
            {
                Projectile.velocity = (player.Center - Projectile.Center).RotatedBy(MathHelper.ToRadians(rotTimer)) / 8;
            }
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(rotTimer));
            if (AITimer - MoveTime > RetractTime)
            {
                Projectile.Kill();
            }

            AITimer++;
        }

        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (player is null || !player.active || player.statLife == 0) return;
            Vector2 drawPos = player.Center;
            Vector2 PlayerToProj = Projectile.Center - player.Center;
            int segmentHeight = 34;
            float distanceLeft = PlayerToProj.Length() + segmentHeight / 2;
            float rotation = PlayerToProj.ToRotation();
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle secondFrame = texture.Frame(1, 3, 0, 1);

            while (distanceLeft > 0f)
            {
                drawPos += PlayerToProj.SafeNormalize(Vector2.Zero) * segmentHeight;
                distanceLeft = drawPos.Distance(Projectile.Center);
                distanceLeft -= segmentHeight;
                Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, secondFrame, Color.White, rotation, new Vector2(17, 17), 1f, SpriteEffects.None);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            float nan = float.NaN;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center, Projectile.Center, 17, ref nan))
            {
                return true;
            }

            return false;
        }
    }
}
