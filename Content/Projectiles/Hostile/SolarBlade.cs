using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class SolarBlade : ModProjectile
    {
        float AITimer = 0;
        ref float Mode => ref Projectile.ai[0];
        ref float ChampID => ref Projectile.ai[1];
        ref float TimeToFire => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 75;
            Projectile.height = 75;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
            Projectile.alpha = 255;
            DrawOffsetX = -15;
            DrawOriginOffsetY = -20;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<SolarBurn>(), 120);
        }

        public override void AI()
        {
            switch (Mode)
            {
                case 0:
                    Projectile.velocity = Main.npc[(int)ChampID].Center.DirectionTo(Projectile.Center);
                    break;
                case 1:
                    if (TimeToFire == 0)
                    {
                        Projectile.velocity *= 20;
                        Projectile.netUpdate = true;
                    }
                    TimeToFire--;
                    break;
            }
            if (AITimer == 0)
            {
                Projectile.scale = 0.01f;
            }
            if (Projectile.scale < 1)
            {
                Projectile.scale += 2f / 30f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (AITimer < 30 && Projectile.alpha > 0 && Projectile.timeLeft > 30)
            {
                Projectile.alpha -= 255 / 20;
            }

            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha += 255 / 30;
            }

            AITimer++;
            Lighting.AddLight(Projectile.Center, new Vector3(100, 100, 100));
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare);
            }
        }
    }
}
