using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Content.Biomes;
using Paracosm.Content.Buffs;
using Paracosm.Content.Items.Armor.Celestial;
using Paracosm.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Players
{
    public class ParacosmPlayer : ModPlayer
    {
        public bool paracosmicHelmet = false;
        public bool paracosmicHelmetSet = false;
        public bool paracosmicGoggles = false;
        public bool paracosmicGogglesSet = false;
        public bool windWarriorBreastplate = false;
        public bool championsCrownSet = false;
        float solarExplosionTimer = 600;

        public bool sunCoin = false;
        public bool corruptedDragonHeart = false;
        public bool parashardSigil = false;
        float paraSigilHitTimer = 120;
        bool paraSigilActiveTimer = false;

        public bool paracosmicHelmetBuff = false;
        public bool paracosmicGogglesBuff = false;

        public bool infected = false;
        public bool paracosmicBurn = false;
        public bool solarBurn = false;
        public bool solarExplosion = false;

        public override void ResetEffects()
        {
            paracosmicHelmet = false;
            paracosmicHelmetSet = false;
            paracosmicGoggles = false;
            paracosmicGogglesSet = false;
            windWarriorBreastplate = false;
            championsCrownSet = false;

            sunCoin = false;
            corruptedDragonHeart = false;
            parashardSigil = false;

            paracosmicHelmetBuff = false;
            paracosmicGogglesBuff = false;

            infected = false;
            paracosmicBurn = false;
            solarBurn = false;
            solarExplosion = false;
        }

        public override void PostUpdateEquips()
        {
            if (sunCoin)
            {
                Player.AddBuff(BuffID.Sunflower, 10);
            }

            if (windWarriorBreastplate)
            {
                Player.wingTimeMax += Player.wingTimeMax / 2;
            }

            if (paracosmicHelmetBuff)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.Shiverthorn, 0, 1);
                }
                Player.statDefense += 20;
                Player.endurance += 0.15f;
            }

            if (paracosmicGogglesBuff)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.BlueTorch, Main.rand.Next(-10, 10), Main.rand.Next(-10, 10));
                }
                Player.GetDamage(DamageClass.Magic) += 20f / 100f;
                Player.GetDamage(DamageClass.Summon) += 20f / 100f;
                Player.manaCost -= 40f / 100f;
            }

            if (paracosmicHelmet && paracosmicHelmetSet)
            {
                if (Player.statLife <= (Player.statLifeMax2 / 2))
                {
                    Player.AddBuff(ModContent.BuffType<ParacosmicHelmetBuff>(), 10);
                }

                if (Player.statLife > (Player.statLifeMax2 / 2))
                {
                    Player.ClearBuff(ModContent.BuffType<ParacosmicHelmetBuff>());
                }
            }

            if (paracosmicGoggles && paracosmicGogglesSet)
            {
                if (Player.statLife <= (Player.statLifeMax2 / 2))
                {
                    Player.AddBuff(ModContent.BuffType<ParacosmicGogglesBuff>(), 10);
                }

                if (Player.statLife > (Player.statLifeMax2 / 2))
                {
                    Player.ClearBuff(ModContent.BuffType<ParacosmicGogglesBuff>());
                }
            }

            if (championsCrownSet)
            {
                if (KeybindSystem.SolarExplosion.JustPressed && !Player.HasBuff(ModContent.BuffType<SolarExplosionCooldown>()))
                {
                    Player.AddBuff(ModContent.BuffType<SolarExplosionCooldown>(), 3600);
                    solarExplosionTimer = 600;
                    foreach (var enemy in Main.ActiveNPCs)
                    {
                        if (!enemy.friendly)
                        {
                            enemy.AddBuff(ModContent.BuffType<SolarBurn>(), 600);
                        }
                    }
                }
                if (Player.HasBuff(ModContent.BuffType<SolarExplosionCooldown>()))
                {
                    if (solarExplosionTimer > 0 && solarExplosionTimer % 5 == 0)
                    {
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            var proj = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center + new Vector2(Main.rand.Next(-1200, 1200), Main.rand.Next(-1200, 1200)), Vector2.Zero, ProjectileID.SolarCounter, 300, 2);
                        }
                    }
                    solarExplosionTimer--;
                }      
            }

            if (corruptedDragonHeart)
            {
                Player.buffImmune[BuffID.CursedInferno] = true;
                Player.buffImmune[BuffID.Ichor] = true;
            }

            if (parashardSigil)
            {
                if (paraSigilActiveTimer == true)
                {
                    paraSigilHitTimer--;
                }
                if (paraSigilHitTimer == 0)
                {
                    paraSigilHitTimer = 120;
                    paraSigilActiveTimer = false;
                };
            }
        }

        public override void PostUpdate()
        {
            if (Player.InModBiome<ParacosmicDistortion>())
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    Terraria.Graphics.Effects.Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(152, 152, 255));
                }
            }
            else
            {
                if (Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate("DivineSeekerShader");
                }
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (parashardSigil)
            {
                if (paraSigilActiveTimer == false && paraSigilHitTimer > 0)
                {
                    paraSigilActiveTimer = true;
                    for (int i = 0; i < 5; i++)
                    {
                        Projectile.NewProjectile(Item.GetSource_None(), Player.MountedCenter, Main.rand.NextVector2Circular(1, 1).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<HomingBlueFire>(), 100 + info.Damage, info.Knockback);
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (corruptedDragonHeart)
            {
                target.AddBuff(BuffID.CursedInferno, 180);
                target.AddBuff(BuffID.Ichor, 180);
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (infected || paracosmicBurn || solarBurn)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;

                if (infected)
                {
                    Player.lifeRegen -= 100;
                }

                if (paracosmicBurn)
                {
                    Player.lifeRegen -= 24;
                }

                if (solarBurn)
                {
                    Player.lifeRegen -= 50;
                }

            }
        }
    }
}
