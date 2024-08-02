using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class BlueFireBall : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 2 });
            }
            AITimer++;
            if (AITimer % 10 == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                }
            }
            Projectile.rotation = AITimer;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParacosmicBurn>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = (Projectile.oldPos[i] - Main.screenPosition) + drawOrigin;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(Texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
