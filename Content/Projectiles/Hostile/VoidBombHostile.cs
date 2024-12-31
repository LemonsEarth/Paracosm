using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VoidBombHostile : ModProjectile
    {
        float AITimer = 0;
        ref float TimeToBoom => ref Projectile.ai[0];
        ref float ProjAmount => ref Projectile.ai[1];
        ref float ProjSpeedMod => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.5f });
            LemonUtils.DustCircle(Projectile.Center, 16, 10, DustID.Granite, 1.2f);
            for (int i = 0; i < 9; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-5, 5)), Main.rand.Next(61, 64), Main.rand.NextFloat(0.8f, 1.2f));
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = (int)(-ProjAmount / 2); i <= (int)(ProjAmount / 2); i++)
                {
                    if (ProjSpeedMod == 0)
                    {
                        return;
                    }
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2((i + 1) * 5, 10 * ProjSpeedMod), ModContent.ProjectileType<VoidBoltHostile>(), Projectile.damage, Projectile.knockBack, ai0: 60 / ProjSpeedMod, ai1:  5 * ProjSpeedMod, ai2: 60 / ProjSpeedMod);
                }
            }
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie67 with { Volume = 0.5f });
            }
            if (AITimer >= TimeToBoom)
            {
                Projectile.Kill();
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, AITimer / TimeToBoom);
            Projectile.rotation = MathHelper.ToRadians(AITimer * Projectile.velocity.Length());
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 2)
                {
                    Projectile.frame = 0;
                }
            }
            AITimer++;
        }
    }
}
