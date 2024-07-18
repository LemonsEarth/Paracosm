using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Paracosm.Content.Items.Armor;
namespace Paracosm.Content.Buffs
{
    public class ParacosmicHelmetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmicHelmetBuffPlayer>().paracosmicHelmetBuff = true;
        }
    }

    public class ParacosmicHelmetBuffPlayer : ModPlayer
    {
        public bool paracosmicHelmetBuff = false;

        public override void ResetEffects()
        {
            paracosmicHelmetBuff = false;
        }

        public override void UpdateEquips()
        {
            if (!paracosmicHelmetBuff)
            {
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Shiverthorn, 0, 1);
            }
            Player.statDefense += 20;
            Player.endurance += 0.15f;
        }
    }
}
