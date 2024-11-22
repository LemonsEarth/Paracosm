using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class TrackingTeslaShot : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float Target => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap);
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
                SoundEngine.PlaySound(SoundID.Item94 with { PitchRange = (0.5f, 0.7f) });
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueMoss);

            Lighting.AddLight(Projectile.Center, 0, 80, 80);
            Projectile.rotation = Projectile.velocity.ToRotation();
            //Projectile.velocity += Projectile.Center.DirectionTo(Main.player[(int)Target].Center);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {

        }
    }
}
