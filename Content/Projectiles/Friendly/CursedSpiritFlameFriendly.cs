using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class CursedSpiritFlameFriendly : ModProjectile
    {
        ref float SpawnTime => ref Projectile.ai[0];
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
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            AIType = 0;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                Projectile.scale = 0.1f;
                SoundEngine.PlaySound(SoundID.Zombie53 with { MaxInstances = 8, PitchVariance = 1.0f, Volume = 0.2f }, Projectile.Center);
            }
            if (AITimer < SpawnTime)
            {
                if (Projectile.scale < 1f)
                {
                    Projectile.scale += 1 / SpawnTime;
                }
            }
            if (AITimer > 0 && AITimer % SpawnTime == 0)
            {
                Vector2 position = Vector2.Zero;
                if (Projectile.owner == Main.myPlayer)
                {
                    position = Main.MouseWorld;
                }
                Projectile.velocity = (position - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 300);
        }
    }
}
