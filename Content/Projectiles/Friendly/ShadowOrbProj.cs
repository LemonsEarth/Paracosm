using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Weapons.Magic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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

        //bool doGray = false;

        //public override void OnSpawn(IEntitySource source)
        //{
        //    if (source is EntitySource_ItemUse_WithAmmo sourceItem && sourceItem.Entity is Item item && item.type == ModContent.ItemType<VoidcoreStaff>())
        //    {
        //        doGray = true;
        //    }
        //}

        //public override Color? GetAlpha(Color lightColor)
        //{
        //    if (!doGray) return null;
        //    float luminance = 0.3f * lightColor.R + 0.59f * lightColor.G + 0.11f * lightColor.B;
        //    Color gray = new Color(luminance, luminance, luminance);
        //    return Color.Lerp(lightColor, gray, 0.5f);
        //}

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
