using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Common.Utils;
using Paracosm.Content.Biomes.Overworld;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Buffs;
using Paracosm.Content.Buffs.Cooldowns;
using Paracosm.Content.Projectiles.Friendly;
using Terraria;
using Terraria.Graphics.Shaders;
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
        public bool vortexControlUnitSet = false;

        public bool sunCoin = false;
        public bool corruptedDragonHeart = false;
        public bool parashardSigil = false;
        float paraSigilHitTimer = 120;
        bool paraSigilActiveTimer = false;

        public bool paracosmicHelmetBuff = false;
        public bool paracosmicGogglesBuff = false;
        public bool nebulousPower = false;

        public bool infected = false;
        public bool paracosmicBurn = false;
        public bool solarBurn = false;
        public bool melting = false;
        public bool vortexForce = false;

        public override void ResetEffects()
        {
            paracosmicHelmet = false;
            paracosmicHelmetSet = false;
            paracosmicGoggles = false;
            paracosmicGogglesSet = false;
            windWarriorBreastplate = false;
            championsCrownSet = false;
            vortexControlUnitSet = false;

            sunCoin = false;
            corruptedDragonHeart = false;
            parashardSigil = false;

            paracosmicHelmetBuff = false;
            paracosmicGogglesBuff = false;
            nebulousPower = false;

            infected = false;
            paracosmicBurn = false;
            solarBurn = false;
            melting = false;
            vortexForce = false;
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
                if (KeybindSystem.ChampionsCrown.JustPressed && !Player.HasBuff(ModContent.BuffType<ChampionsCrownCooldown>()))
                {
                    Player.AddBuff(ModContent.BuffType<MeltingDebuff>(), 1200);
                    Player.AddBuff(ModContent.BuffType<ChampionsCrownCooldown>(), 5400);
                    Player.AddBuff(ModContent.BuffType<SolarBurn>(), 240);

                    foreach (var enemy in Main.ActiveNPCs)
                    {
                        if (enemy.CanBeChasedBy() && enemy.Center.Distance(Player.Center) < 1200)
                        {
                            enemy.AddBuff(ModContent.BuffType<MeltingDebuff>(), 600);
                            enemy.AddBuff(ModContent.BuffType<SolarBurn>(), 1200);
                            if (Main.myPlayer == Player.whoAmI)
                            {
                                var proj = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), enemy.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 600, 2);
                            }
                        }
                    }
                    LemonUtils.DustCircle(Player.Center, 16, 5, DustID.SolarFlare, Main.rand.NextFloat(0.8f, 1.2f));
                }
            }

            if (nebulousPower)
            {
                Player.moveSpeed *= 2f;
                if (!Player.controlJump)
                {
                    Player.gravity = Player.defaultGravity * 2f;
                }
                if ((Player.controlLeft && Player.velocity.X > 0) || (Player.controlRight && Player.velocity.X < 0))
                {
                    Player.runSlowdown = 10f;
                }

                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GemAmethyst);
                    dust.velocity = (-Vector2.UnitY * Main.rand.NextFloat(2f, 7f)).RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15)));
                    dust.noGravity = true;
                    Dust dust2 = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GemDiamond);
                    dust2.velocity = (-Vector2.UnitY * Main.rand.NextFloat(2f, 7f)).RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 15)));
                    dust2.noGravity = true;
                }
            }

            if (melting)
            {
                Player.statDefense *= 0;
            }

            if (vortexControlUnitSet)
            {
                if (KeybindSystem.VortexControl.JustPressed && !Player.HasBuff(ModContent.BuffType<VortexForceCooldown>()))
                {
                    Player.AddBuff(ModContent.BuffType<VortexForce>(), 600);
                    Player.AddBuff(ModContent.BuffType<VortexForceCooldown>(), 4800);
                }
            }

            if (vortexForce)
            {
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if (proj.friendly && !proj.IsMinionOrSentryRelated && proj.owner == Player.whoAmI)
                    {
                        proj.velocity += proj.Center.DirectionTo(Main.MouseWorld) * 1.5f;
                    }
                }
                if (Main.myPlayer == Player.whoAmI)
                {
                    LemonUtils.DustCircle(Main.MouseWorld, 8, 2, DustID.HallowSpray, Main.rand.NextFloat(0.8f, 1.2f));
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
            if (Player.InModBiome<VoidMid>() || Player.InModBiome<VoidHigh>() || Player.InModBiome<VoidLow>())
            {
                Player.moonLordMonolithShader = true;

                if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:DarknessShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    ScreenShaderData shader = Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:DarknessShader").GetShader();
                    if (Player.InModBiome<VoidHigh>())
                    {
                        shader.Shader.Parameters["distance"].SetValue(0.20f);
                    }
                    else if (Player.InModBiome<VoidMid>())
                    {
                        shader.Shader.Parameters["distance"].SetValue(0.10f);
                    }
                    else
                    {
                        shader.Shader.Parameters["distance"].SetValue(0.5f);
                    }
                    shader.Shader.Parameters["maxGlow"].SetValue(10);
                }
            }
            else
            {
                if (Terraria.Graphics.Effects.Filters.Scene["Paracosm:DarknessShader"].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate("Paracosm:DarknessShader");
                }
            }

            if (Player.InModBiome<ParacosmicDistortion>())
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:ScreenTintShader").GetShader().UseColor(new Color(152, 152, 255));
                    Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].GetShader().UseProgress(1);
                }
            }
            else
            {
                if (Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate("Paracosm:ScreenTintShader");
                }
            }

            if (melting)
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:ScreenTintShader").GetShader().UseColor(new Color(255, 192, 100));
                    Terraria.Graphics.Effects.Filters.Scene["Paracosm:ScreenTintShader"].GetShader().UseProgress(1);
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
