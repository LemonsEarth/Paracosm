using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Paracosm.Content.Projectiles.Friendly;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.HeldProjectiles
{
    public class UnboundDarkProj : ModProjectile
    {
        int AITimer = 0;
        ref float rotDirection => ref Projectile.ai[0]; // 1 or -1
        ref float MousePosX => ref Projectile.ai[1];
        ref float MousePosY => ref Projectile.ai[2];
        int AttackCount = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 86;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
        }

        float rotMul = 1;
        float rot = 0f;
        Vector2 playerToMouse;
        float timeMul = 0.1f;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;

            Vector2 playerToProj = player.Center.DirectionTo(Projectile.Center);
            if (AITimer == 0)
            {
                Projectile.Opacity = 0f;
               playerToMouse = player.Center.DirectionTo(new Vector2(MousePosX, MousePosY));
            }
            Vector2 offset = playerToMouse.RotatedBy(-rotDirection * MathHelper.PiOver2) * (122 / 2); // root(2 * 86^2)
            float rot = rotDirection * AITimer * (9 * player.GetAttackSpeed(DamageClass.Melee) * rotMul) * timeMul;
            if (Math.Abs(rot) < 30)
            {
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, rot / 30);
            }
            else if (Math.Abs(rot) > 150)
            {
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, (rot - 150) / 30);
            }
            if (Math.Abs(rot) >= 180)
            {
                Projectile.Kill();
            }
            Vector2 pos = player.Center + offset.RotatedBy(MathHelper.ToRadians(rot));

            Projectile.Center = pos;
            Projectile.rotation = playerToProj.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity = Vector2.Zero;
            if (AITimer % 3 == 0 && AttackCount < 5)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AttackCount++;
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, playerToProj * 15, ModContent.ProjectileType<LightsEndBeam>(), Projectile.damage / 5, 1);
                }
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, Scale: Main.rand.NextFloat(1f, 1.4f));
            timeMul += 0.1f;
            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(DrawOffsetX, Projectile.gfxOffY);
                Color color = Color.Black * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                color.A /= 2;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (Projectile.oldSpriteDirection[k] == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, spriteEffects, 0);
            }
            return true;
        }
    }
}
