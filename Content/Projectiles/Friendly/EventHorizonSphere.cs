using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class EventHorizonSphere : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float PosIndex => ref Projectile.ai[1];
        ref float Distance => ref Projectile.ai[2];
        bool released = false;
        bool fired = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 255 / 60;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item92 with { MaxInstances = 2 });
            }

            player.manaRegen = 0;

            bool usedMana = true;
            if (AITimer % 15 == 0)
            {
                usedMana = player.CheckMana(2, true, true);
            }
            if (!player.channel || !usedMana) released = true;

            if (player.channel && !released)
            {
                player.SetDummyItemTime(2);
                Projectile.timeLeft = 60;
                if (Distance < 600)
                {
                    Distance += AITimer / 2;
                }
                Projectile.Center = player.Center + (Vector2.UnitY * Distance).RotatedBy(PosIndex * MathHelper.PiOver4).RotatedBy(MathHelper.ToRadians(AITimer * 3));
                NPC closestNPC = LemonUtils.GetClosestNPC(Projectile);
                if (closestNPC != null)
                {
                    if (player.Center.Distance(closestNPC.Center) < player.Center.Distance(Projectile.Center))
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            if (AITimer % 30 == 0)
                            {
                                Vector2 direction = Projectile.Center.DirectionTo(closestNPC.Center);
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, direction * 15, ModContent.ProjectileType<EventHorizonProj>(), Projectile.originalDamage / 2, 0.5f);
                            }
                        }
                    }
                }
            }

            if (released)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    if (!fired)
                    {
                        Vector2 mouseDir = Main.MouseWorld - Projectile.Center;
                        Projectile.velocity = mouseDir.SafeNormalize(Vector2.Zero) * 50;
                        fired = true;
                    }
                    Projectile.netUpdate = true;
                }
            }

            Lighting.AddLight(Projectile.Center, 100, 20, 100);
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst);
            dust.noGravity = true;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Rectangle drawRectangle = texture.Frame(1, Main.projFrames[Type], 0, 1);

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, drawRectangle, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
