using System;
using System.Collections.Generic;
using System.IO;
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
    public class SolarHammer : ModProjectile
    {
        float AITimer = 0;

        ref float ShotCount => ref Projectile.ai[0];
        public Vector2 TargetPosition
        {
            get { return new Vector2(Projectile.ai[1], Projectile.ai[2]); }
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }
        bool reachedPos = false;
        int attackTimer = 0;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(reachedPos);
            writer.Write(attackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            reachedPos = reader.ReadBoolean();
            attackTimer = reader.ReadInt32();
        }

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
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            DrawOffsetX = -18;
        }

        public override void AI()
        {
            if (!reachedPos)
            {
                Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.Length());
                if (Projectile.Center.Distance(TargetPosition) < 50)
                {
                    reachedPos = true;
                    Projectile.timeLeft = (int)ShotCount * 60;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.rotation = MathHelper.ToRadians(AITimer * 4);
                Projectile.velocity = Vector2.Zero;
                if (attackTimer == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, -10).RotatedBy(i * MathHelper.PiOver4), ModContent.ProjectileType<SolarFireball>(), Projectile.damage, 0);
                        }
                    }
                    ShotCount--;
                    if (ShotCount == 0)
                    {
                        Projectile.Kill();
                    }
                    attackTimer = 60;
                }
                attackTimer--;
            }
            if (AITimer < 30 && Projectile.alpha > 0)
            {
                Projectile.alpha -= 255 / 20;
            }
            AITimer++;
            Lighting.AddLight(Projectile.Center, 100, 80, 0);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14);
        }
    }
}
