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
    public class CursedSpiritFlame : ModProjectile
    {
        ref float SpawnTime => ref Projectile.ai[0];
        Vector2 direction
        {
            get => new Vector2(Projectile.ai[1], Projectile.ai[2]);
            set
            {
                Projectile.ai[1] = value.X;
                Projectile.ai[2] = value.Y;
            }
        }
        int AITimer = 0;
        public float speed = 1;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(speed);
            writer.Write(AITimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {

            speed = reader.ReadSingle();
            AITimer = reader.ReadInt32();
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            AIType = 0;
            Projectile.timeLeft = 480;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.scale = 0.1f;
            }
            if (AITimer < SpawnTime)
            {
                if (Projectile.scale < 1f)
                {
                    Projectile.scale += 1 / SpawnTime;
                }
            }
            if (AITimer == SpawnTime)
            {
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
                Projectile.netUpdate = true;
            }
            for (int i = 0; i < 2; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Scale: 1.5f);
                dust.noGravity = true;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
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

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.CursedInferno, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);
        }
    }
}
