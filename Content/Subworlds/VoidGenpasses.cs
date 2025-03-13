using Paracosm.Common.Utils;
using Paracosm.Content.Items.Accessories;
using StructureHelper.API;
using StructureHelper;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using StructureHelper.Models;

namespace Paracosm.Content.Subworlds
{
    public class VoidGenPass : GenPass
    {
        public VoidGenPass() : base("Void", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void";
            Main.worldSurface = Main.maxTilesY - 2000;
            Main.rockLayer = Main.maxTilesY - 1000;
        }
    }

    public class VoidTerrainGenPass : GenPass
    {
        public VoidTerrainGenPass() : base("Terrain", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void Terrain";

            //Amount of "Terrain"
            for (int splotches = 0; splotches < 30; splotches++)
            {
                int tiles = Main.rand.Next(100, 500);
                int x = Main.rand.Next(0, 1000);
                int y = Main.rand.Next(1000, 2000);
                double strength = Main.rand.Next(5, 8);
                int steps = Main.rand.Next(60, 80);
                int dirX = Main.rand.Next(-1, 2);
                int dirY = Main.rand.Next(-1, 2);

                for (int i = 0; i < tiles; i++)
                {
                    if (i % 25 == 0)
                    {
                        dirX = Main.rand.Next(-1, 2);
                        dirY = Main.rand.Next(-1, 2);
                    }
                    int truePosX = x + i * dirX;
                    int truePosY = y + i * dirY;
                    if ((truePosX > 450 && truePosX < 550) && (truePosY > 1450 && truePosY < 1550))
                    {
                        continue;
                    }
                    WorldGen.TileRunner(truePosX, truePosY, strength, steps, TileID.ShimmerBrick, true);
                }
                progress.Set(splotches / 50f);
            }

            // Ground
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    int posX = i * 10;
                    int posY = 2600 + j * 100;
                    WorldGen.TileRunner(posX, posY, 10 + Main.rand.Next(-3, 5), 40 + Main.rand.Next(-10, 10), TileID.ShimmerBrick, true);
                }
            }

        }
    }

    public class VoidStructuresGenPass : GenPass
    {
        public VoidStructuresGenPass() : base("Structures", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void Structures";
            Point16 dims = MultiStructureGenerator.GetStructureDimensions("Content/Structures/VoidBuilding", Paracosm.Instance, 0);
            int amountX = (Main.maxTilesX / dims.X) - 1;
            for (int depthMul = 0; depthMul < 5; depthMul++)
            {
                for (int i = 0; i < amountX; i++)
                {
                    int x = 20 + (dims.X * i) + Main.rand.Next(0, 60);
                    int numBuildings = Main.rand.Next(1, 5);
                    for (int j = 0; j < numBuildings; j++) //num of building stacked on top of each other
                    {
                        int y = 2600 + (depthMul * 100) + (j * dims.Y) - 1;
                        if (x + dims.X > 1000 || y + dims.Y > 3000) continue;
                        MultiStructureData structureData = MultiStructureGenerator.GetMultiStructureData("Content/Structures/VoidBuilding", Paracosm.Instance);
                        if (!MultiStructureGenerator.IsInBounds(structureData, 0, new Point16(x, y))) continue;
                        MultiStructureGenerator.GenerateMultistructureRandom("Content/Structures/VoidBuilding", new Point16(x, y), Paracosm.Instance);
                    }
                }
            }
        }
    }

    public class VoidChestGenPass : GenPass
    {
        public VoidChestGenPass() : base("Chests", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void Chests";
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                int items = Main.rand.Next(8, 16);
                Chest chest = Main.chest[chestIndex];
                if (chest == null)
                {
                    continue;
                }

                for (int inventoryIndex = 1; inventoryIndex < items; inventoryIndex++)
                {
                    if (Main.rand.Next(0, 100) == 1)
                    {
                        int randEmblem = Main.rand.NextFromList(ModContent.ItemType<ChampionEmblem>(), ModContent.ItemType<RaiderEmblem>(), ModContent.ItemType<WarlockEmblem>(), ModContent.ItemType<CommanderEmblem>());
                        chest.item[inventoryIndex].SetDefaults(randEmblem);
                        continue;
                    }
                    int randItemID = LemonUtils.GetRandomItemID();
                    if (randItemID == ItemID.Zenith)
                    {
                        continue;
                    }
                    chest.item[inventoryIndex].SetDefaults(randItemID);
                    if (chest.item[inventoryIndex].maxStack > 1)
                    {
                        chest.item[inventoryIndex].stack = Main.rand.Next(24, 87);
                    }
                }
            }
        }
    }

    public class VoidPaintBlackGenPass : GenPass
    {
        public VoidPaintBlackGenPass() : base("Paint", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void Paint";

            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Main.tile[i, j].HasTile)
                    {
                        Tile tile = Main.tile[i, j];
                        if (tile.TileType == TileID.ShimmerBrick)
                        {
                            tile.TileColor = PaintID.ShadowPaint;
                        }
                        else
                        {
                            tile.TileColor = PaintID.BlackPaint;
                        }
                    }
                }
            }
        }
    }
}
