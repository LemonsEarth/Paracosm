using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Buffs;
using Paracosm.Content.Projectiles.Friendly;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.HeldProjectiles
{
    public class ParashardSwordProjectile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float attackSpeed => ref Projectile.ai[1];
        Vector2 mousePos = Vector2.Zero;
        Vector2 mouseDir = Vector2.Zero;
        int direction = 0;
        bool canSpin = true;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(canSpin);
            writer.Write(direction);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            canSpin = reader.ReadBoolean();
            direction = reader.ReadInt32();
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.damage = 50;
            Projectile.timeLeft = 120;

            Projectile.hide = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            attackSpeed = 1;
            Player player = Main.player[Projectile.owner];
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            if (AITimer == 0)
            {
                direction = player.direction;
            }
            AITimer++;
            if (AITimer < 60)
            {
                attackSpeed += 0.008f * player.GetAttackSpeed(DamageClass.Melee);
            }

            if (AITimer == 60)
            {
                SoundEngine.PlaySound(SoundID.MaxMana);
            }

            if (player.channel && canSpin)
            {
                player.SetDummyItemTime(2);
                Projectile.timeLeft = 120 + (int)(player.GetAttackSpeed(DamageClass.Melee) * 100 / 2);
                Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
                Projectile.rotation = Vector2.UnitX.RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee)).ToRotation();
                Projectile.Center = playerCenter + new Vector2(55, -55).RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee));
                if (AITimer % 10 == 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpookyWood, 1, 4, Scale: 1f);
                    }
                }
            }
            else if (!player.channel && canSpin && AITimer >= 60 && Projectile.owner == Main.myPlayer)
            {
                mousePos = Main.MouseWorld;
                mouseDir = (mousePos - Projectile.Center).SafeNormalize(Vector2.Zero);
                canSpin = false;
                SoundEngine.PlaySound(SoundID.Item1);
                Projectile.netUpdate = true;
            }
            else if (!player.channel && canSpin && AITimer < 60)
            {
                Projectile.Kill();
            }
            if (canSpin == false)
            {
                if (Projectile.Center.Distance(mousePos) > 20)
                {
                    Projectile.velocity = mouseDir * 20 * player.GetAttackSpeed(DamageClass.Melee);
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    if (AITimer % 30 == 0 && Projectile.owner == Main.myPlayer)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16)), ModContent.ProjectileType<ParaSwordShard>(), 30, 2);
                        }
                    }
                }
                Projectile.rotation = Vector2.UnitX.RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee)).ToRotation();
            }

            if (AITimer % 10 == 0)
            {
                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(Texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.position, new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16)), ModContent.ProjectileType<ParaSwordShard>(), Projectile.damage / 4, 2);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(target.position, target.width, target.height, DustID.SpookyWood, 1, 4, Scale: 1.5f);
            }
            target.AddBuff(ModContent.BuffType<ParacosmicBurn>(), 600);
        }
    }
}
