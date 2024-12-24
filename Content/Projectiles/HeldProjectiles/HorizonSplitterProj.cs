using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.HeldProjectiles
{
    public class HorizonSplitterProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float DistanceIndex => ref Projectile.ai[1];
        ref float MouseSide => ref Projectile.ai[2];

        float distance => Projectile.height * 0.75f;

        float rotated = 0;
        const float maxRotation = 360;

        bool spawnedProj = false;
        int savedDamage = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 138;
            Projectile.height = 138;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            
            Projectile.timeLeft = 10000;
            DrawOffsetX = 45;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;

            if (MouseSide < 0)
            {
                Projectile.spriteDirection = -1;
            }
            if (AITimer == 0)
            {
                savedDamage = Projectile.damage;
                Projectile.scale = 0.1f;
                LemonUtils.DustCircle(Projectile.position, 16, 10, DustID.SolarFlare, 2f, true);
                SoundEngine.PlaySound(SoundID.Item92 with { PitchRange = (-0.3f, 0.3f) });
            }
            if (Projectile.scale < 1)
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.2f);
            }
            if (Projectile.alpha > 0 && AITimer < 10)
            {
                Projectile.alpha -= 255 / 10;
            }

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IchorTorch);
            Vector2 playerToProj = player.Center.DirectionTo(Projectile.Center);
            Projectile.rotation = playerToProj.ToRotation() + MathHelper.PiOver2;
            if (player.channel && rotated == 0)
            {
                Projectile.timeLeft = 180;
                player.itemAnimation = 2;
                player.itemTime = 2;
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = player.Center + new Vector2(-MouseSide, -1) * distance * DistanceIndex;
                Projectile.damage = 0;
                if (AITimer == (int)(60 / player.GetAttackSpeed(DamageClass.Melee)) && DistanceIndex < 5)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + playerToProj * distance, Vector2.Zero, Type, savedDamage, 6, ai1: DistanceIndex + 1, ai2: MouseSide);
                    }
                }
            }
            else
            {
                Projectile.Center = player.Center + new Vector2(-MouseSide, -1).RotatedBy(MathHelper.ToRadians(rotated)) * DistanceIndex * distance;
                Projectile.damage = savedDamage * (int)DistanceIndex;
                rotated += 6 * MouseSide * player.GetAttackSpeed(DamageClass.Melee);
            }
            if (Math.Abs(rotated) >= maxRotation)
            {
                Projectile.Kill();
            }

            if (rotated > 300)
            {
                Projectile.alpha += 255 / 60;
            }

            AITimer++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SolarBurn>(), 60);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { MaxInstances = 2 });
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(DrawOffsetX, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
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
