using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class FadedFatesProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float Homing => ref Projectile.ai[1];
        float speed = 0;
        NPC closestNPC;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Granite);
            dust.noGravity = true;
            if (AITimer == 0 && Homing > 0)
            {
                Projectile.penetrate = 3;
                Projectile.timeLeft = 150;
                Projectile.netUpdate = true;
            }
            if (AITimer >= 30 && Homing > 0)
            {
                if (speed < 7)
                {
                    speed += 0.1f;
                }
                closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    Projectile.velocity += Projectile.Center.DirectionTo(closestNPC.Center) * speed;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            if (Homing > 0) return;
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, PitchRange = (-0.3f, 0.3f) }, Projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, -Vector2.UnitY.RotatedBy(MathHelper.ToRadians(120 * i)) * 10, Type, Projectile.damage, Projectile.knockBack, ai1: 1);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DarkBleed>(), 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, k);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                color.A /= 2;
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
