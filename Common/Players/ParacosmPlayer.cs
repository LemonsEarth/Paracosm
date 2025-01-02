using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Common.Utils;
using Paracosm.Content.Biomes.Overworld;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Buffs;
using Paracosm.Content.Buffs.Cooldowns;
using Paracosm.Content.Items.Accessories;
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
        public bool stardustTailSet = false;

        public bool sunCoin = false;
        public bool corruptedDragonHeart = false;
        public bool corruptedLifeCrystal = false;
        public bool voidHeart = false;
        public bool voidCharm = false;
        public bool voidPendant = false;
        public bool masterEmblem = false;
        public bool parashardSigil = false;
        float paraSigilHitTimer = 120;
        bool paraSigilActiveTimer = false;
        public bool nebulousEnergy = false;
        public bool starfallCoating = false;
        public bool craterCoating = false;
        public bool spiritCoating = false;
        public bool universalCoating = false;
        public bool commandersWill = false;
        public bool secondHand = false;
        public bool oathOfVengeance = false;
        public bool steelSight = false;
        public bool organicSight = false;
        public bool infiltratorMark = false;
        public int sentryCount = 0;

        int oathTimer = 0;
        const int OATH_COOLDOWN = 1200;

        int hitTimer = 0;
        const int HIT_TIMER_CD = 10;

        public bool paracosmicHelmetBuff = false;
        public bool paracosmicGogglesBuff = false;
        public bool nebulousPower = false;
        public bool branchedOfLifed = false;

        public bool infected = false;
        public bool paracosmicBurn = false;
        public bool solarBurn = false;
        public bool melting = false;
        public bool vortexForce = false;
        public bool voidTerror = false;
        int voidTerrorTimer = 0;

        public override void ResetEffects()
        {
            paracosmicHelmet = false;
            paracosmicHelmetSet = false;
            paracosmicGoggles = false;
            paracosmicGogglesSet = false;
            windWarriorBreastplate = false;
            championsCrownSet = false;
            vortexControlUnitSet = false;
            stardustTailSet = false;

            sunCoin = false;
            corruptedDragonHeart = false;
            corruptedLifeCrystal = false;
            parashardSigil = false;
            nebulousEnergy = false;
            voidHeart = false;
            voidCharm = false;
            voidPendant = false;
            starfallCoating = false;
            craterCoating = false;
            spiritCoating = false;
            universalCoating = false;
            commandersWill = false;
            sentryCount = 0;
            secondHand = false;
            oathOfVengeance = false;
            steelSight = false;
            organicSight = false;
            masterEmblem = false;
            infiltratorMark = false;

            paracosmicHelmetBuff = false;
            paracosmicGogglesBuff = false;
            nebulousPower = false;
            branchedOfLifed = false;

            infected = false;
            paracosmicBurn = false;
            solarBurn = false;
            melting = false;
            vortexForce = false;
            voidTerror = false;

            // Dash
            
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[2] < 15 && Player.doubleTapCardinalTimer[3] == 0)
            {
                DashDir = 1;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[3] < 15 && Player.doubleTapCardinalTimer[2] == 0)
            {
                DashDir = -1;
            }
            else
            {
                DashDir = 0;
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (corruptedLifeCrystal)
            {
                healValue += 40;
            }
            if (voidHeart)
            {
                healValue += 70;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hitTimer > 0)
            {
                return; //On hit effects proc on timer
            }
            if (starfallCoating)
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    Vector2 pos = target.Center - new Vector2(Main.rand.Next(-50, 50), 800 + Main.rand.Next(-50, 50));
                    Vector2 velocity = pos.DirectionTo(target.Center) * (StarfallCoating.STAR_VELOCITY + Main.rand.NextFloat(-2f, 2f));
                    int damage = item.damage / 2;
                    if (damage > StarfallCoating.STAR_DAMAGE_CAP)
                    {
                        damage = StarfallCoating.STAR_DAMAGE_CAP;
                    }
                    Projectile.NewProjectile(Player.GetSource_OnHit(target), pos, velocity, ProjectileID.HallowStar, damage, 0f);
                }
            }

            if (craterCoating)
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    Vector2 pos = target.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20));
                    int damage = item.damage;
                    if (damage > CraterCoating.EXPLOSION_DAMAGE_CAP)
                    {
                        damage = CraterCoating.EXPLOSION_DAMAGE_CAP;
                    }
                    Projectile.NewProjectile(Player.GetSource_OnHit(target), pos, Vector2.Zero, ModContent.ProjectileType<CraterExplosion>(), damage, 0f);
                }
            }

            if (spiritCoating)
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    int damage = item.damage;
                    if (damage > SpiritCoating.SPIRIT_DAMAGE_CAP)
                    {
                        damage = SpiritCoating.SPIRIT_DAMAGE_CAP;
                    }
                    int projID = ModContent.ProjectileType<SpiritProjDamage>();
                    if (Main.rand.NextBool(4))
                    {
                        projID = ModContent.ProjectileType<SpiritProjHeal>();
                    }

                    Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.UnitY.RotatedByRandom(2 * MathHelper.Pi) * SpiritCoating.SPIRIT_VELOCITY, projID, damage, 0f, ai0: 60);
                }
            }

            if (universalCoating)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = target.Center - new Vector2(Main.rand.NextFloat(-400, 400), 800 + Main.rand.Next(-100, 100));
                    int damage = item.damage;
                    Vector2 velocity = pos.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(Main.rand.Next(-30, 30))) * (UniversalCoating.METEOR_VELOCITY + Main.rand.NextFloat(-4f, 4f));
                    Projectile.NewProjectile(item.GetSource_OnHit(target), pos, velocity, ProjectileID.Meteor1, damage, 0f, ai1: 1f); // ai1 is scale
                }
            }

            hitTimer = HIT_TIMER_CD;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Player.whoAmI && hit.DamageType.CountsAsClass(DamageClass.Melee) && proj.whoAmI == Player.heldProj && hitTimer == 0)
            {
                if (starfallCoating)
                {
                    Vector2 pos = target.Center - new Vector2(Main.rand.Next(-100, 100), 800 + Main.rand.Next(-100, 100));
                    Vector2 velocity = pos.DirectionTo(target.Center) * (StarfallCoating.STAR_VELOCITY + Main.rand.NextFloat(-2f, 2f));
                    int damage = proj.damage / 2;
                    if (damage > StarfallCoating.STAR_DAMAGE_CAP)
                    {
                        damage = StarfallCoating.STAR_DAMAGE_CAP;
                    }
                    Projectile.NewProjectile(Player.GetSource_OnHit(target), pos, velocity, ProjectileID.HallowStar, damage, 0f);
                }

                if (craterCoating)
                {
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Vector2 pos = target.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20));
                        int damage = proj.damage;
                        if (damage > CraterCoating.EXPLOSION_DAMAGE_CAP)
                        {
                            damage = CraterCoating.EXPLOSION_DAMAGE_CAP;
                        }
                        Projectile.NewProjectile(Player.GetSource_OnHit(target), pos, Vector2.Zero, ModContent.ProjectileType<CraterExplosion>(), damage, 0f);
                    }
                }

                if (spiritCoating)
                {
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        int damage = proj.damage;
                        if (damage > SpiritCoating.SPIRIT_DAMAGE_CAP)
                        {
                            damage = SpiritCoating.SPIRIT_DAMAGE_CAP;
                        }
                        int projID = ModContent.ProjectileType<SpiritProjDamage>();
                        if (Main.rand.NextBool(4))
                        {
                            projID = ModContent.ProjectileType<SpiritProjHeal>();
                        }

                        Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.UnitY.RotatedByRandom(2 * MathHelper.Pi) * SpiritCoating.SPIRIT_VELOCITY, projID, damage, 0f, ai0: 60);
                    }
                }

                if (universalCoating)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 pos = target.Center - new Vector2(Main.rand.NextFloat(-400, 400), 800 + Main.rand.Next(-100, 100));
                        int damage = proj.damage;
                        Vector2 velocity = pos.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(Main.rand.Next(-30, 30))) * (UniversalCoating.METEOR_VELOCITY + Main.rand.NextFloat(-4f, 4f));
                        Projectile.NewProjectile(Player.GetSource_OnHit(target), pos, velocity, ProjectileID.Meteor1, damage, 0f, ai0: 1f, ai1: 1f, ai2: 10f);
                    }
                }

                hitTimer = HIT_TIMER_CD;
            }

            if (Main.myPlayer == Player.whoAmI)
            {
                if (steelSight)
                {
                    if (hit.Crit && hit.DamageType.CountsAsClass(DamageClass.Ranged) && Main.rand.NextBool(8))
                    {
                        Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center, Player.Center.DirectionTo(Main.MouseWorld) * 20, proj.type, proj.damage / 3, proj.knockBack, ai0: proj.ai[0], ai1: proj.ai[1], ai2: proj.ai[2]);
                    }
                }
                if (organicSight)
                {
                    if (hit.Crit && hit.DamageType.CountsAsClass(DamageClass.Ranged))
                    {
                        if (Main.rand.NextBool(4))
                        {
                            Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center, Player.Center.DirectionTo(Main.MouseWorld) * 22, proj.type, proj.damage / 2, proj.knockBack, ai0: proj.ai[0], ai1: proj.ai[1], ai2: proj.ai[2]);
                        }
                        if (Main.rand.NextBool(15))
                        {
                            Player.Heal(5);
                        }
                    }
                }
            }
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
                    if (nebulousEnergy)
                    {
                        Player.AddBuff(ModContent.BuffType<MeltingDebuff>(), 1000);
                        Player.AddBuff(ModContent.BuffType<ChampionsCrownCooldown>(), 2700);
                        Player.AddBuff(ModContent.BuffType<SolarBurn>(), 180);
                    }
                    else
                    {
                        Player.AddBuff(ModContent.BuffType<MeltingDebuff>(), 1200);
                        Player.AddBuff(ModContent.BuffType<ChampionsCrownCooldown>(), 5400);
                        Player.AddBuff(ModContent.BuffType<SolarBurn>(), 240);
                    }

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

            if (oathOfVengeance)
            {
                if (oathTimer > 0)
                {
                    oathTimer--;
                }
            }

            if (voidTerror)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.SolarFlare, newColor: Color.Black);
                if (Main.rand.NextBool(250))
                {
                    int randDebuff = Main.rand.NextFromList(20, 22, 23, 24, 31, 32, 33, 35, 36, 37, 38, 39, 46, 47, 67, 68, 69, 70, 80, 144, 149, 153, 156, 195, 196, 197,
                                                            ModContent.BuffType<SolarBurn>(), ModContent.BuffType<ParacosmicBurn>(), ModContent.BuffType<MeltingDebuff>());
                    Player.AddBuff(randDebuff, 120); 
                }

                if (!Player.HasBuff(BuffID.PotionSickness) && Main.rand.NextBool(300)) // Separate chance to inflict potion sickness
                {
                    Player.AddBuff(BuffID.PotionSickness, 600);
                }

                if (Main.rand.NextBool(300)) // Chance to remove minions and sentries
                {
                    Player.maxTurrets = 0;
                    Player.maxMinions = 0;
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
                    if (nebulousEnergy)
                    {
                        Player.AddBuff(ModContent.BuffType<VortexForce>(), 800);
                        Player.AddBuff(ModContent.BuffType<VortexForceCooldown>(), 2400);
                    }
                    else
                    {
                        Player.AddBuff(ModContent.BuffType<VortexForce>(), 600);
                        Player.AddBuff(ModContent.BuffType<VortexForceCooldown>(), 4800);
                    }
                }
            }

            if (vortexForce)
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    foreach (var proj in Main.ActiveProjectiles)
                    {
                        if (proj.friendly && !proj.IsMinionOrSentryRelated && proj.owner == Player.whoAmI)
                        {
                            proj.velocity += proj.Center.DirectionTo(Main.MouseWorld) * 1.5f;
                        }
                    }
                    LemonUtils.DustCircle(Main.MouseWorld, 8, 2, DustID.HallowSpray, Main.rand.NextFloat(0.8f, 1.2f));
                }
            }

            if (corruptedDragonHeart)
            {
                Player.buffImmune[BuffID.CursedInferno] = true;
                Player.buffImmune[BuffID.Ichor] = true;
            }

            if (parashardSigil || voidPendant)
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

            if ((craterCoating || spiritCoating || starfallCoating) && hitTimer > 0)
            {
                hitTimer--;
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
                        shader.Shader.Parameters["distance"].SetValue(0.4f);
                    }
                    else if (Player.InModBiome<VoidMid>())
                    {
                        shader.Shader.Parameters["distance"].SetValue(0.4f);
                    }
                    else
                    {
                        shader.Shader.Parameters["distance"].SetValue(0.8f);
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

            if (voidTerror)
            {
                if (voidTerrorTimer < 95)
                {
                    voidTerrorTimer++;
                }
                ScreenShaderData shader = Terraria.Graphics.Effects.Filters.Scene.Activate("Paracosm:DarknessShader").GetShader();
                shader.Shader.Parameters["distance"].SetValue(1 - (voidTerrorTimer / 100f));
                shader.Shader.Parameters["maxGlow"].SetValue(1f);
            }
            else
            {
                voidTerrorTimer = 0;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (parashardSigil)
            {
                if (paraSigilActiveTimer == false && paraSigilHitTimer > 0)
                {
                    paraSigilActiveTimer = true;
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Projectile.NewProjectile(Item.GetSource_None(), Player.MountedCenter, Main.rand.NextVector2Circular(1, 1).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<HomingBlueFire>(), 50 + info.Damage, info.Knockback);
                        }
                    }
                }
            }

            if (voidCharm)
            {
                if (Main.myPlayer == Player.whoAmI)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(Player.GetSource_FromAI(), Player.MountedCenter, (Vector2.UnitY * 20).RotatedBy(MathHelper.PiOver2 * i), ModContent.ProjectileType<VoidBomb>(), 250 + info.Damage, info.Knockback);
                    }
                }
            }

            if (voidPendant)
            {
                if (paraSigilActiveTimer == false && paraSigilHitTimer > 0)
                {
                    paraSigilActiveTimer = true;
                    if (Main.myPlayer == Player.whoAmI)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Projectile.NewProjectile(Player.GetSource_FromAI(), Player.MountedCenter, (Vector2.UnitY * 20).RotatedBy(MathHelper.PiOver4 * i), ModContent.ProjectileType<VoidBomb>(), 300 + info.Damage, info.Knockback);
                        }
                    }
                }
            }

            if (oathOfVengeance)
            {
                if (oathTimer <= 0)
                {
                    Player.AddBuff(BuffID.ParryDamageBuff, 120);
                    oathTimer = OATH_COOLDOWN;
                }
            }

            if (masterEmblem && Main.myPlayer == Player.whoAmI)
            {
                if (Main.rand.NextBool(2) && !voidTerror)
                {
                    Player.AddBuff(ModContent.BuffType<VoidTerrorDebuff>(), 600);
                }
            }
        }

        public override void PostHurt(Player.HurtInfo info)
        {
            if (voidPendant)
            {
                Player.AddImmuneTime(info.CooldownCounter, 100);
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

        public override void UpdateLifeRegen()
        {
            if (branchedOfLifed)
            {
                Player.lifeRegen += 3;
            }

            if (corruptedLifeCrystal || voidHeart)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
            }
        }

        public override void PostUpdateBuffs()
        {
            if (branchedOfLifed)
            {
                Player.GetDamage(DamageClass.Generic) += 10f / 100f;
                Player.GetCritChance(DamageClass.Generic) += 10f;
            }
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (corruptedLifeCrystal || voidHeart)
            {
                regen *= 0;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (corruptedLifeCrystal || voidHeart)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
            }

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

        int DashDelay = 0;
        const int DashCD = 30;

        int DashDir = 0;
        const int DashVelocity = 20;
        int DashTimer = 0;
        const int DashDuration = 30;
        public override void PreUpdateMovement()
        {
            if (!infiltratorMark || Player.mount.Active)
            {
                return;
            }

            if (DashDelay == 0 && DashDir != 0)
            {
                Player.velocity = new Vector2(DashDir * DashVelocity, Player.velocity.Y);
                DashDelay = DashCD;      
                DashTimer = DashDuration;
                LemonUtils.DustCircle(Player.Center, 16, 5, DustID.Granite, 1.3f);
            }

            if (DashTimer > 0)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Granite);
                DashTimer--;
            }
            
            if (DashDelay > 0)
            {
                DashDelay--;
            }
        }
    }
}
