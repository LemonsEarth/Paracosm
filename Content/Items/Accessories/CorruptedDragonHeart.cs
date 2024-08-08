using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Microsoft.Build.Execution;

namespace Paracosm.Content.Items.Accessories
{
    public class CorruptedDragonHeart : ModItem
    {
        const int maxLifeBoost = 10;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(maxLifeBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statLifeMax2 += player.statLifeMax2 / maxLifeBoost;
            player.GetModPlayer<CorruptedDragonHeartPlayer>().corruptedDragonHeart = true;
        }
    }

    public class CorruptedDragonHeartPlayer : ModPlayer
    {
        public bool corruptedDragonHeart = false;

        public override void ResetEffects()
        {
            corruptedDragonHeart = false;
        }

        public override void PostUpdateEquips()
        {
            if (!corruptedDragonHeart)
            {
                return;
            }

            Player.buffImmune[BuffID.CursedInferno] = true;
            Player.buffImmune[BuffID.Ichor] = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!corruptedDragonHeart)
            {
                return;
            }
            target.AddBuff(BuffID.CursedInferno, 180);
            target.AddBuff(BuffID.Ichor, 180);
        }
    }
}
