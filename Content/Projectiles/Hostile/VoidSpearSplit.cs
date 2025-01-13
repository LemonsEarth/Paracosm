using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Common.Utils;
using Paracosm.Content.Bosses.TheNameless;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Hostile
{
    public class VoidSpearSplit : ModProjectile
    {
        public override string Texture => "Paracosm/Content/Projectiles/Hostile/VoidSpearHostile";

        int AITimer = 0;
        ref float WaitTime => ref Projectile.ai[0];
        ref float SplitInterval => ref Projectile.ai[1];
        ref float SplitCount => ref Projectile.ai[2];

        public int NPCOwner;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 66;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.light = 1f;
            DrawOffsetX = -75;
            DrawOriginOffsetX = 30;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPCOwner);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPCOwner = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 2, DustID.Granite);
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 7;
            }

            NPC owner = Main.npc[NPCOwner];
            if (owner is null || !owner.active || owner.type != ModContent.NPCType<TheNameless>())
            {
                Projectile.Kill();
                return;
            }
            TheNameless nameless = (TheNameless)owner.ModNPC;

            if (AITimer < WaitTime)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = Projectile.Center.DirectionTo(nameless.player.Center).ToRotation();
            }
            else if (AITimer == WaitTime)
            {
                Projectile.velocity = Projectile.Center.DirectionTo(nameless.player.Center) * 30;
                SoundEngine.PlaySound(SoundID.Item1 with { MaxInstances = 2, Volume = 1.2f, PitchRange = (-0.1f, 0.3f) }, Projectile.Center);

            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (AITimer % SplitInterval == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.PiOver2) * 10, ModContent.ProjectileType<VoidBoltSplit>(), Projectile.damage, 1f, ai0: 60, ai1: SplitCount);
                        }
                    }                
                }
            }

            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Granite);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f + DrawOriginOffsetX, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(DrawOffsetX, DrawOriginOffsetY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
