﻿using System;
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
                    SoundEngine.PlaySound(SoundID.NPCDeath13 with { MaxInstances = 3 });
                }
                Projectile.velocity = (Main.player[(int)PlayerIDToTrack].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 6;
                if (AITimer % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * 0.5f, Projectile.type, Projectile.damage / 2, 0f, ai1: 0, ai2: 0);
                    Projectile.netUpdate = true;
                }
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, Scale: 1.2f);

            Projectile.rotation = AITimer;
            AITimer++;
        }
    }
}