using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;

namespace Paracosm.Content.Buffs
{
    public class ParacosmicBurn : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmicBurnPlayer>().paracosmicBurn = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ParacosmicBurnNPC>().paracosmicBurn = true;
        }
    }

    public class ParacosmicBurnPlayer : ModPlayer
    {
        public bool paracosmicBurn = false;

        public override void ResetEffects()
        {
            paracosmicBurn = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (!paracosmicBurn)
            {
                return;
            }

            if (Player.lifeRegen > 0)
            {
                Player.lifeRegen = 0;
            }
            Player.lifeRegenTime = 0;

            Player.lifeRegen -= 24;
        }
    }

    public class ParacosmicBurnNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool paracosmicBurn = false;

        public override void ResetEffects(NPC npc)
        {
            paracosmicBurn = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!paracosmicBurn)
            {
                return;
            }

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
    }
}
