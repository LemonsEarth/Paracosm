﻿using Paracosm.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class ParacosmicFlameHostile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 24;
            Projectile.frameCounter = 2;
            Projectile.penetrate = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void OnSpawn(IEntitySource source)
        {

        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ParacosmicBurn>(), 120);
        }

        public override void AI()
        {
            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { MaxInstances = 1, Pitch = -0.1f }, Projectile.Center);
            }
            AITimer++;
            Projectile.alpha += 10;
            Projectile.rotation = AITimer / Main.rand.Next(2, 4);
            Projectile.frameCounter--;
            if (Projectile.frameCounter == 0)
            {
                if (Projectile.frame < 2)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 2;
                }
                else if (Projectile.frame >= 2 && Projectile.frame < 6)
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 6;
                }
                else
                {
                    Projectile.frame++;
                    Projectile.frameCounter = 1;
                }
            }
            if (AITimer % 2 == 0)
            {
                Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.IceTorch);
            }
        }
    }
}
