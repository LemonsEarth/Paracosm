using Paracosm.Content.Biomes.Overworld;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Bosses.InfectedRevenant;
using Paracosm.Content.Bosses.NebulaMaster;
using Paracosm.Content.Bosses.StardustLeviathan;
using Paracosm.Content.Bosses.VortexMothership;
using Paracosm.Content.Buffs;
using Paracosm.Content.NPCs.Hostile.Paracosmic;
using Paracosm.Content.NPCs.Hostile.Void;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool paracosmicBurn = false;
        public bool solarBurn = false;
        public bool melting = false;
        public bool darkBleed = false;

        public override void ResetEffects(NPC npc)
        {
            paracosmicBurn = false;
            solarBurn = false;
            melting = false;
            darkBleed = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (paracosmicBurn)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= 150;
                if (damage < 150)
                {
                    damage = 150;
                }
            }


            if (darkBleed)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= 50;
                if (damage < 50)
                {
                    damage = 50;
                }
            }


            if (solarBurn)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                npc.lifeRegen -= 300;
                if (damage < 300)
                {
                    damage = 300;
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            if (darkBleed)
            {
                hurtInfo.Damage -= (hurtInfo.Damage / 20);
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (melting)
            {
                modifiers.Defense.Base *= 0;
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.SolarFlare);
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff<StardustTailDebuff>())
            {
                float projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
                modifiers.FlatBonusDamage += StardustTailDebuff.TagDamageBoost * projTagMultiplier;
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<ParacosmicDistortion>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<Wanderer>(), 0.3f);
                pool.Add(ModContent.NPCType<ParastoneRoller>(), 0.1f);
            }

            if (spawnInfo.Player.InModBiome<VoidMid>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<ShadowSeeker>(), 0.05f);
                pool.Add(ModContent.NPCType<VoidWormHead>(), 0.01f);
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (npc.type == ModContent.NPCType<VortexMothership>() || npc.type == ModContent.NPCType<VortexTeslaGun>() || npc.type == ModContent.NPCType<InfectedRevenantCorruptHead>() || npc.type == ModContent.NPCType<InfectedRevenantCrimsonHead>() || npc.type == ModContent.NPCType<NebulaMaster>() || npc.type == ModContent.NPCType<StardustLeviathanHead>())
            {
                return false;
            }
            return true;
        }
    }
}
