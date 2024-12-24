using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class NightActualSlash : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float randomValue => ref Projectile.ai[1];
        ref float randomValue2 => ref Projectile.ai[2];
        int savedDamage;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.timeLeft = 200;
            
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 200 && Projectile.owner == Main.myPlayer)
            {
                randomValue2 = Main.rand.Next(30, 60);
                Projectile.netUpdate = true;
                savedDamage = Projectile.damage;
            }
            if (Projectile.timeLeft > 200 - randomValue2)
            {
                Projectile.alpha = 255;
                Projectile.damage = 0;
                return;
            }

            if (Projectile.timeLeft == 200 - randomValue2)
            {
                Projectile.alpha = 0;
                if (Projectile.owner == Main.myPlayer)
                {
                    randomValue = Main.rand.Next(-180, 180);
                    Projectile.rotation = MathHelper.ToRadians(randomValue);
                    Projectile.netUpdate = true;
                }
                SoundEngine.PlaySound(SoundID.Item60 with { MaxInstances = 2 });
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, -Projectile.velocity.X, -Projectile.velocity.Y);
            }
            Projectile.damage = savedDamage;
            int frameDur = 4;
            AITimer++;
            Player player = Main.player[Projectile.owner];
            Projectile.frameCounter++;
            if (Projectile.frameCounter == frameDur)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame == 4)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
