using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class BloodBlast : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float Tracking => ref Projectile.ai[1];
        ref float PlayerIDToTrack => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (Tracking > 0)
            {
                if (AITimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { MaxInstances = 1 }, Projectile.Center);
                }
                Projectile.velocity = (Main.player[(int)PlayerIDToTrack].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 6;
                if (AITimer % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.5f, Projectile.type, Projectile.damage / 2, 0f, ai1: 0, ai2: 0);
                }
            }
            else
            {
                if (AITimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { MaxInstances = 1 }, Projectile.Center);
                }
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ichor, Scale: 1.0f);

            Lighting.AddLight(Projectile.Center, 0.9f, 0.9f, 0.0f);
            Projectile.rotation = AITimer;
            AITimer++;
        }
    }
}
