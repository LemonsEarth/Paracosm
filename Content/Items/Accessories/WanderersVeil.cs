using Paracosm.Common.Players;
using Paracosm.Common.Systems;
using Paracosm.Content.Buffs;
using Paracosm.Content.Buffs.Cooldowns;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class WanderersVeil : ModItem
    {
        public static readonly float DRBoost = 20;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DRBoost);

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 56;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 5);
            Item.rare = ItemRarityID.Purple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (KeybindSystem.WanderersVeil.JustPressed && !player.HasBuff(ModContent.BuffType<WanderersVeilCooldown>()))
            {
                player.AddBuff(ModContent.BuffType<WanderersVeilBuff>(), 600);
                player.AddBuff(ModContent.BuffType<WanderersVeilCooldown>(), 2400);
            }
        }
    }
}
