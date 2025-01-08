using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class EquinoxMoon : ModProjectile
    {
        public override string Texture => "Paracosm/Assets/Textures/FX/Empty32Tex";
        static readonly string SunTexture = "Paracosm/Content/Projectiles/Friendly/EquinoxMoon";
        int AITimer = 0;

        ref float SunID => ref Projectile.ai[0];
        ref float PosX => ref Projectile.ai[1];
        ref float PosY => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void OnSpawn(IEntitySource source)
        {
            PosX = Main.rand.NextBool().ToDirectionInt();
            PosY = Main.rand.NextBool().ToDirectionInt();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.SetDummyItemTime(2);
            Projectile.timeLeft = 2;
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 255 / 60;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item92 with { MaxInstances = 2 }, Projectile.Center);
            }

            if (AITimer == 30)
            {
                Projectile.damage *= 2;
                Projectile.netUpdate = true;
            }

            if (AITimer % 30 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    player.CheckMana(30, true, true);
                    Vector2 mouseDir = player.Center.DirectionTo(Main.MouseWorld);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, mouseDir * 4, ModContent.ProjectileType<EquinoxMoonProj>(), Projectile.damage, 4f, ai0: 30f);
                }
            }

            if (player.channel)
            {
                float rotSpeedDeg = AITimer * 4;
                Projectile.rotation = MathHelper.ToRadians(rotSpeedDeg);
                float distance = 33.94f; //(float)Math.Sqrt(2 * (Projectile.width * Projectile.width))
                Vector2 offset = new Vector2(PosX, PosY).SafeNormalize(Vector2.Zero);
                Projectile.Center = player.MountedCenter + (offset * distance * 2).RotatedBy(MathHelper.ToRadians(rotSpeedDeg));
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (Projectile.Colliding(Projectile.Hitbox, Main.projectile[(int)SunID].Hitbox))
                    {
                        player.CheckMana(80, true, true);
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Vector2.UnitY * 16).RotatedBy(MathHelper.PiOver4 * i), ModContent.ProjectileType<EquinoxMoonProj>(), Projectile.damage, 4f, ai0: 30f);
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Vector2.UnitY * 16).RotatedBy(MathHelper.PiOver4 * i + MathHelper.PiOver4 / 2), ModContent.ProjectileType<EquinoxSunProj>(), Projectile.damage, 4f, ai0: 30f);
                        }
                    }
                    else
                    {
                        player.CheckMana(30, true, true);
                        Vector2 mouseDir = player.Center.DirectionTo(Main.MouseWorld);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, mouseDir * 12, ModContent.ProjectileType<EquinoxMoonProj>(), Projectile.damage, 4f, ai0: 30f);
                    }
                }
                Projectile.Kill();
            }

            Lighting.AddLight(Projectile.Center, 2, 2, 2);
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond);
            dust.noGravity = true;

            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item90 with { PitchRange = (0.2f, 0.5f) }, Projectile.Center);
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.GemDiamond, 1.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(SunTexture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
