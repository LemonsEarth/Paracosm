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
    public class ParacosmicGogglesBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ParacosmicGogglesBuffPlayer>().paracosmicGogglesBuff = true;
        }
    }

    public class ParacosmicGogglesBuffPlayer : ModPlayer
    {
        public bool paracosmicGogglesBuff = false;

        public override void ResetEffects()
        {
            paracosmicGogglesBuff = false;
        }

        public override void UpdateEquips()
        {
            if (!paracosmicGogglesBuff)
            {
                return;
            }
            Player.GetDamage(DamageClass.Magic) += 20f / 100f;
            Player.GetDamage(DamageClass.Summon) += 20f / 100f;
            Player.manaCost -= 40f / 100f;
        }
    }
}
