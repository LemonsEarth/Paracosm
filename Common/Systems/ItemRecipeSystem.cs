using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Paracosm.Content.Bosses;
using Paracosm.Content.Items.Consumables;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Paracosm.Content.Items.Accessories;

namespace Paracosm.Common.Systems
{
    public class ItemRecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            Recipe.Create(ItemID.HermesBoots)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CloudinaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.BlizzardinaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.SandstorminaBottle)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.FrogLeg)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.Aglet)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.AnkletoftheWind)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ShoeSpikes)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ClimbingClaws)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.FlyingCarpet)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.LavaCharm)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.LuckyHorseshoe)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.ObsidianRose)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.NaturesGift)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CreativeWings)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 2)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.BandofRegeneration)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 1)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.MagmaStone)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 3)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.PhilosophersStone)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.CrossNecklace)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();
            Recipe.Create(ItemID.StarVeil)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 5)
                .AddTile(TileID.DemonAltar)
                .Register();

            Recipe.Create(ItemID.MagicQuiver)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.TitanGlove)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.MoonCharm)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.PutridScent)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.FleshKnuckles)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            Recipe.Create(ItemID.FrozenTurtleShell)
                .AddIngredient(ModContent.ItemType<DemonCoin>(), 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
