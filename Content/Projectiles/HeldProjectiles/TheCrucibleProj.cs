using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Buffs;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.HeldProjectiles
{
    public class TheCrucibleProj : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        bool released = false;
        float projCount = 0;
        float AttackTimer = 0;
        Vector2 mousePos = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 38;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 10000;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            if (Main.myPlayer == Projectile.owner)
            {
                mousePos = Main.MouseWorld;
            }
            Projectile.Center = player.Center + new Vector2((int)(mousePos - player.Center).SafeNormalize(Vector2.Zero).X * 60, -40);
            Projectile.spriteDirection = (int)(mousePos - player.Center).SafeNormalize(Vector2.Zero).X;

            if (player.channel && !released)
            {
                switch (AITimer)
                {
                    case < 60:
                        projCount += 0.05f;
                        Dust.NewDust(mousePos, 1, 1, DustID.Torch);
                        break;
                    case < 120:
                        projCount += 0.1f;
                        Dust.NewDust(mousePos, 1, 1, DustID.GemTopaz);
                        break;
                    case 200:
                        LemonUtils.DustCircle(player.Center, 16, 10, DustID.IchorTorch);
                        break;
                    case > 120 and < 240:
                        projCount += 0.2f;
                        Dust.NewDust(mousePos, 1, 1, DustID.SolarFlare);
                        break;
                    case 240 or 300:
                        LemonUtils.DustCircle(player.Center, 16, 10, DustID.IchorTorch, 1.5f);
                        break;
                    case > 300 and < 400:
                        Dust.NewDust(mousePos, 1, 1, DustID.SolarFlare);
                        player.AddBuff(BuffID.Burning, 2);
                        break;
                    case > 480:
                        Dust.NewDust(mousePos, 1, 1, DustID.SolarFlare);
                        player.AddBuff(ModContent.BuffType<SolarBurn>(), 2);
                        break;
                }
            }
            if (!player.channel)
            {
                released = true;
            }

            if (released)
            {
                if (AttackTimer <= 0)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Main.player[Projectile.owner].CheckMana(15, true, true);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), mousePos, Vector2.Zero, ModContent.ProjectileType<SolarExplosionMagic>(), Projectile.damage * (int)(projCount / 5) + 69, Projectile.knockBack);
                    }
                    switch (projCount)
                    {
                        case > 20:
                            AttackTimer = 3;
                            break;
                        case > 10:
                            AttackTimer = 5;
                            break;
                        case > 5:
                            AttackTimer = 10;
                            break;
                    }
                    projCount--;
                }

                if (projCount <= 0)
                {
                    Projectile.Kill();
                }
                AttackTimer--;
            }

            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, Projectile.gfxOffY);
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
