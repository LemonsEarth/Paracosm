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
    public class Infected : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<InfectedPlayer>().infected = true;
        }
    }

    public class InfectedPlayer : ModPlayer
    {
        public bool infected = false;

        public override void ResetEffects()
        {
            infected = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (!infected)
            {
                return;
            }

            if (Player.lifeRegen > 0)
            {
                Player.lifeRegen = 0;
            }
            Player.lifeRegenTime = 0;

            Player.lifeRegen -= 100;
        }
    }
}
