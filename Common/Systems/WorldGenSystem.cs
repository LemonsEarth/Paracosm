using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using StructureHelper;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Paracosm.Content.Tiles;
using System;

namespace Paracosm.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        private void GenerateCorruptTower()
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;
            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/CorruptTower", Mod, ref dims);
                int x = WorldGen.genRand.Next(0 + dims.X, Main.maxTilesX - dims.X);
                int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, (int)GenVars.worldSurfaceHigh);
                Tile tile = Main.tile[x, y];
                point = new Point16(x, y);
                if (tile.HasTile && (tile.TileType == TileID.CorruptGrass || tile.TileType == TileID.Ebonstone || tile.TileType == TileID.Ebonsand))
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/CorruptTower", point, Mod);
            }
        }

        private void GenerateCrimsonHouse()
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;
            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/CrimsonHouse", Mod, ref dims);
                int x = WorldGen.genRand.Next(0 + dims.X, Main.maxTilesX - dims.X);
                int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, (int)GenVars.worldSurfaceHigh);
                Tile tile = Main.tile[x, y];
                point = new Point16(x, y);
                if (tile.HasTile && (tile.TileType == TileID.CrimsonGrass || tile.TileType == TileID.Crimstone || tile.TileType == TileID.Crimsand))
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/CrimsonHouse", point, Mod);
            }
        }

        private void GenerateJungleHouse(GenerationProgress progress, GameConfiguration config)
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;
            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/JungleHouse", Mod, ref dims);
                int x = WorldGen.genRand.Next(0 + dims.X, Main.maxTilesX - dims.X);
                int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, (int)GenVars.worldSurfaceHigh);
                Tile tile = Main.tile[x, y];
                Tile tileAbove = Main.tile[x, y - 1];
                point = new Point16(x, y);
                if (tile.HasTile && (tile.TileType == TileID.Mud || tile.TileType == TileID.JungleGrass) && !tileAbove.HasTile)
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/JungleHouse", point, Mod);
            }
        }

        private void GenerateIceCastle(GenerationProgress progress, GameConfiguration config)
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;

            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/IceCastle", Mod, ref dims);
                int x = WorldGen.genRand.Next(0 + dims.X, Main.maxTilesX - dims.X);
                int y = WorldGen.genRand.Next((int)(Main.rockLayer + dims.Y), Main.maxTilesY - 200);
                Tile tile = Main.tile[x, y];
                point = new Point16(x, y);
                if ((tile.HasTile && (tile.TileType == TileID.IceBlock || tile.TileType == TileID.SnowBlock)) && (!CheckForTiles(x, y, dims.X, dims.Y, ModContent.TileType<ParastoneBlock>()) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.BlueDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.GreenDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.PinkDungeonBrick)))
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/IceCastle", point, Mod);
            }
        }

        private void GenerateParacosmicDistortionCore(GenerationProgress progress, GameConfiguration config)
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;

            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/ParacosmicDistortionCore", Mod, ref dims);
                int x = WorldGen.genRand.Next((int)((float)Main.maxTilesX * (1f / 3f)), (int)((float)Main.maxTilesX * (2f / 3f)));
                int y = WorldGen.genRand.Next((int)GenVars.rockLayer, (Main.maxTilesY - 200) / 2);

                point = new Point16(x, y);
                if (!CheckForTiles(x, y, dims.X, dims.Y, TileID.LihzahrdBrick) && !CheckForTiles(x, y, dims.X, dims.Y, ModContent.TileType<ParastoneBlock>()) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.BlueDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.GreenDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.PinkDungeonBrick))
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/ParacosmicDistortionCore", point, Mod);
            }
        }

        private void GenerateParacosmicDistortionCoreLarge(GenerationProgress progress, GameConfiguration config)
        {
            bool successfulGen = false;
            Point16 point = Point16.Zero;

            while (!successfulGen)
            {
                Point16 dims = Point16.Zero;
                Generator.GetDimensions("Content/Structures/ParacosmicCoreLarge", Mod, ref dims);
                int x = WorldGen.genRand.Next((int)((float)Main.maxTilesX * (1f / 3f)), (int)((float)Main.maxTilesX * (2f / 3f)));
                int y = WorldGen.genRand.Next((Main.maxTilesY - 200) / 2, Main.maxTilesY - 200);

                point = new Point16(x, y);
                if (!CheckForTiles(x, y, dims.X, dims.Y, TileID.LihzahrdBrick) && !CheckForTiles(x, y, dims.X, dims.Y, ModContent.TileType<ParastoneBlock>()) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.BlueDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.GreenDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.PinkDungeonBrick))
                {
                    successfulGen = true;
                }
            }

            if (successfulGen && point != Point16.Zero)
            {
                Generator.GenerateStructure("Content/Structures/ParacosmicCoreLarge", point, Mod);
            }
        }


        private void GenerateAbandonedArmory(GenerationProgress progress, GameConfiguration config)
        {
            for (int i = 0; i < GetWorldSize() * 2; i++)
            {
                for (int index = 0; index < 5; index++)
                {
                    bool successfulGen = false;
                    Point16 point = Point16.Zero;
                    while (!successfulGen)
                    {
                        Point16 dims = Point16.Zero;
                        Generator.GetMultistructureDimensions("Content/Structures/AbandonedArmory", Mod, index, ref dims);
                        int x = WorldGen.genRand.Next(dims.X, Main.maxTilesX - dims.X);
                        int y = WorldGen.genRand.Next((Main.maxTilesY - 200) / 3, Main.maxTilesY - 200);

                        point = new Point16(x, y);
                        if (!CheckForTiles(x, y, dims.X, dims.Y, TileID.LihzahrdBrick) && !CheckForTiles(x, y, dims.X, dims.Y, ModContent.TileType<ParastoneBlock>()) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.BlueDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.GreenDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.PinkDungeonBrick) && !CheckForTiles(x, y, dims.X, dims.Y, TileID.GrayBrick))
                        {
                            successfulGen = true;
                        }
                    }

                    if (successfulGen && point != Point16.Zero)
                    {
                        Generator.GenerateMultistructureSpecific("Content/Structures/AbandonedArmory", point, Mod, index);
                    }
                }
            }           
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int templeIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Jungle Temple"));
            tasks.Insert(templeIndex + 1, new PassLegacy("Paracosmic Core", GenerateParacosmicDistortionCore));
            tasks.Insert(templeIndex + 1, new PassLegacy("Paracosmic Core", GenerateParacosmicDistortionCore));
            tasks.Insert(templeIndex + 2, new PassLegacy("Large Paracosmic Core", GenerateParacosmicDistortionCoreLarge));
            tasks.Insert(templeIndex + 3, new PassLegacy("Abandoned Armories", GenerateAbandonedArmory));
            tasks.Insert(templeIndex + 4, new PassLegacy("Jungle House", GenerateJungleHouse));
            tasks.Insert(templeIndex + 5, new PassLegacy("Ice Castle", GenerateIceCastle));
        }

        public override void PostWorldGen()
        {
            if (WorldGen.drunkWorldGen)
            {
                GenerateCrimsonHouse();
                GenerateCorruptTower();
            }
            else if (WorldGen.crimson == true)
            {
                GenerateCrimsonHouse();
            }
            else
            {
                GenerateCorruptTower();
            }
        }

        public static bool CheckForTiles(int xCoord, int yCoord, int distanceX, int distanceY, int tileID)
        {
            for (int x = xCoord - distanceX; x < xCoord + distanceX; x++)
            {
                for (int y = yCoord - distanceY; y < yCoord + distanceY; y++)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == tileID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns 1 for Small Worlds, 2 for Medium Worlds, 3 for Large Worlds (and bigger?)
        /// </summary>
        /// <returns></returns>

        public static int GetWorldSize()
        {
            switch (Main.maxTilesX)
            {
                case >= 8400:
                    return 3;
                case >= 6400:
                    return 2;
                default:
                    return 1;
            }
        }
    }
}
