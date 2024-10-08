﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class ShadowOrbProj : ModProjectile
    {
        float AITimer = 60;
        ref float AmmoType => ref Projectile.ai[0];
        ref float RandomRot => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 60;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62);
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 6; i++)
                {
                    RandomRot = Main.rand.Next(-45, 45);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(RandomRot)) * 10, (int)AmmoType, Projectile.damage / 2, Projectile.knockBack);
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
