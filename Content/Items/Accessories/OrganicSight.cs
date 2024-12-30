using Microsoft.Xna.Framework;
using Paracosm.Common.Players;
using Paracosm.Content.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Accessories
{
    public class OrganicSight : ModItem
    {
        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            return incomingItem.type != ModContent.ItemType<SteelSight>();
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 20);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParacosmPlayer>().organicSight = true;

            if (!hideVisual && Main.myPlayer == player.whoAmI)
            {
                float distanceToMouse = player.Center.Distance(Main.MouseWorld);
                float dividend = distanceToMouse / 10;
                Vector2 directionToMouse = player.Center.DirectionTo(Main.MouseWorld);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 pos = player.Center + directionToMouse * (i * dividend);
                    Dust dust = Dust.NewDustDirect(pos, 1, 1, DustID.GemRuby, Scale: 1.5f);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                }
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<SteelSight>());
            recipe.AddIngredient(ModContent.ItemType<Parashard>(), 14);
            recipe.AddIngredient(ItemID.ShroomiteBar, 12);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
