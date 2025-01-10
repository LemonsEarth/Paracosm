using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class ShadowOrbProj : ModProjectile
    {
        float AITimer = 60;
        ref float AmmoType => ref Projectile.ai[0];
        ref float RandomRot => ref Projectile.ai[1];
        ref float Amount => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 60;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            if (Projectile.owner == Main.myPlayer)
            {
                if (Amount == 0) Amount = 6;
                for (int i = 0; i < Amount; i++)
                {
                    RandomRot = Main.rand.Next(-45, 45);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(RandomRot)) * 10, (int)AmmoType, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                }
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            Projectile.velocity /= 1.1f;
            Projectile.rotation += MathHelper.ToRadians(AITimer) * Projectile.velocity.Length();
            AITimer--;
            var dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Shadowflame, Scale: 1.5f);
        }
    }
}
