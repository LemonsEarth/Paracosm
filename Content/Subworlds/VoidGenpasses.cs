using SubworldLibrary;
using StructureHelper;
using Paracosm.Content.Tiles;
using Paracosm.Common.Systems;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;

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
                int tiles = WorldGen.genRand.Next(100, 500);
                int x = WorldGen.genRand.Next(0, 1000);
                int y = WorldGen.genRand.Next(1000, 2000);
                double strength = WorldGen.genRand.Next(5, 8);
                int steps = WorldGen.genRand.Next(60, 80);
                int dirX = WorldGen.genRand.Next(-1, 2);
                int dirY = WorldGen.genRand.Next(-1, 2);

                for (int i = 0; i < tiles; i++)
                {
                    if (i % 25 == 0)
                    {
                        dirX = WorldGen.genRand.Next(-1, 2);
                        dirY = WorldGen.genRand.Next(-1, 2);
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
        }
    }

    public class VoidStructuresGenPass : GenPass
    {
        public VoidStructuresGenPass() : base("Structures", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void Structures";
            for (int i = 0; i < 20; i++)
            {
                int x = (int)MathHelper.Clamp(i * (Main.maxTilesX / 8), 100, 2900);
                int y = (Main.maxTilesY - 1000) + WorldGen.genRand.Next(0, 800);

                Generator.GenerateMultistructureRandom("Content/Structures/VoidStructures", new Point16(x, y), Paracosm.Instance);
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
                int items = WorldGen.genRand.Next(8, 16);
                Chest chest = Main.chest[chestIndex];
                if (chest == null)
                {
                    continue;
                }

                for (int inventoryIndex = 0; inventoryIndex < items; inventoryIndex++)
                {
                    chest.item[inventoryIndex].SetDefaults(LemonUtils.GetRandomNoStackItemID());
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
