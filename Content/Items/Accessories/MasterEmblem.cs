using Paracosm.Content.Items.Rarities;
using Terraria;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Paracosm.Content.Bosses.SolarChampion;
using Paracosm.Content.Items.Materials;
using Paracosm.Common.Players;

namespace Paracosm.Content.Items.Accessories
{
    public class MasterEmblem : ModItem
    {
        static readonly float damageBoost = 40;
        static readonly float critBoost = 35;
        static readonly int manaBoost = 100;
        static readonly int minionBoost = 2;
        static readonly int sentryBoost = 2;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, manaBoost, minionBoost, sentryBoost);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(0, 60);
            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100f;
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.statManaMax2 += manaBoost;
            player.maxMinions += minionBoost;
            player.maxTurrets += sentryBoost;
            player.GetModPlayer<ParacosmPlayer>().masterEmblem = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ChampionEmblem>());
            recipe.AddIngredient(ModContent.ItemType<RaiderEmblem>());
            recipe.AddIngredient(ModContent.ItemType<WarlockEmblem>());
            recipe.AddIngredient(ModContent.ItemType<CommanderEmblem>());
            recipe.AddIngredient(ModContent.ItemType<SolarCore>(), 8);
            recipe.AddIngredient(ModContent.ItemType<VortexianPlating>(), 8);
            recipe.AddIngredient(ModContent.ItemType<UnstableNebulousFlame>(), 8);
            recipe.AddIngredient(ModContent.ItemType<PureStardust>(), 8);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }     
}
