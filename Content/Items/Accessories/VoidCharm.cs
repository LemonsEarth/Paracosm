using Paracosm.Common.Players;
using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories;

public class VoidCharm : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 32;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 8);
        Item.rare = ParacosmRarity.DarkGray;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<ParacosmPlayer>().voidCharm = true;
    }
}
