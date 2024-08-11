using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Paracosm.Content.Projectiles
{
    public class DefiledGlaiveProjectile : ModProjectile
    {
        const int minimumHoldOutRange = 32;
        const int maximumHoldOutRange = 384;

        ref float UseType => ref Projectile.ai[0];


        bool maxReached
        {
            get => Projectile.ai[1] > 0;
            set
            {
                Projectile.ai[1] = value ? 1 : 0;
            }
        }

        ref float rotValue => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        float progress = 0;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;


            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);

            if (Projectile.Center.Distance(player.MountedCenter) >= maximumHoldOutRange - 10)
            {
                maxReached = true;
            }

            switch (UseType)
            {
                case 0:
                    SpearBehaviour(player);
                    break;
                case 1:
                    SpinBehaviour(player);
                    break;
            }

        }

        void SpearBehaviour(Player player)
        {
            if (!maxReached)
            {
                progress += 0.1f;
            }
            else
            {
                progress -= 0.1f;
            }

            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * minimumHoldOutRange, Projectile.velocity * maximumHoldOutRange, progress);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }

        void SpinBehaviour(Player player)
        {
            Projectile.rotation = MathHelper.ToRadians(rotValue * 100);
            Projectile.Center = player.MountedCenter + new Vector2(0, -64).RotatedBy(MathHelper.ToRadians(rotValue * 20));
            rotValue++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i += 2)
            {
                Vector2 drawPos = (Projectile.oldPos[i] - Main.screenPosition) + drawOrigin + new Vector2(0, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(Texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
