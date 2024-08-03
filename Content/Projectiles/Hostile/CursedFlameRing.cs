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
    public class CursedFlameRing : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        Vector2 direction
        {
            get => new Vector2(Projectile.ai[1], Projectile.ai[2]);
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }
        public float speed = 1;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(speed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            speed = reader.ReadSingle();
        }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            AIType = 0;
            Projectile.timeLeft = 480;
            Projectile.tileCollide = false;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
                Projectile.netUpdate = true;
            }
            Projectile.rotation = MathHelper.ToRadians(Projectile.timeLeft * 12);
            for (int i = 0; i < 2; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position - new Vector2(2f, 2f), Projectile.width + 2, Projectile.height + 2, DustID.CursedTorch, Scale: 3f);
                dust.noGravity = true;
            }


            AITimer--;
        }
    }
}
