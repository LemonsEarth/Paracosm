using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles
{
    public class ParashardSwordProjectile : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float attackSpeed => ref Projectile.ai[1];
        Vector2 mousePos = Vector2.Zero;
        Vector2 mouseDir = Vector2.Zero;
        Item heldItem;
        int direction = 0;
        bool canSpin = true;

        public override void SetDefaults()
        {
            Projectile.owner = Main.myPlayer;
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.damage = 50;
            Projectile.timeLeft = 120;
            Projectile.aiStyle = -1;
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
                heldItem = player.HeldItem;
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

            if (player.channel && canSpin && Projectile.owner == Main.myPlayer)
            {
                player.SetDummyItemTime(2);
                Projectile.timeLeft = 120 + (int)(player.GetAttackSpeed(DamageClass.Melee) * 100 / 2);
                Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
                Projectile.rotation = new Vector2(1, 0).RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee)).ToRotation() ;
                Projectile.Center = playerCenter + new Vector2(55, -55).RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee));
                Projectile.netUpdate = true;
                if (AITimer % (int)(attackSpeed * 10) == 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.SpookyWood, 1, 4, Scale: 1f);
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
            if (canSpin == false && Projectile.owner == Main.myPlayer)
            {
                if (Projectile.Center.Distance(mousePos) > 20)
                {
                    Projectile.velocity = mouseDir * 20 * player.GetAttackSpeed(DamageClass.Melee);
                    
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    if (AITimer % 30 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16)), ModContent.ProjectileType<ParaSwordShard>(), 30, 2);
                        }
                    }
                }
                Projectile.rotation = new Vector2(1, 0).RotatedBy(AITimer / 10 * attackSpeed * direction * player.GetAttackSpeed(DamageClass.Melee)).ToRotation();
                Projectile.netUpdate = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.position, new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16), Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(5, 16)), ModContent.ProjectileType<ParaSwordShard>(), Projectile.damage / 4, 2);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(target.Center, target.width, target.height, DustID.SpookyWood, 1, 4, Scale: 1.5f);
            }
            target.AddBuff(ModContent.BuffType<ParacosmicBurn>(), 600);
        }
    }
}
