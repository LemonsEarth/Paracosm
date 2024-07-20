using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using StructureHelper;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;
using Terraria.IO;

namespace Paracosm.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        private void GenerateCorruptTower(GenerationProgress progress, GameConfiguration config)
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

        private void GenerateCrimsonHouse(GenerationProgress progress, GameConfiguration config)
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

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            if (!WorldGen.crimson || WorldGen.drunkWorldGen)
            {
                int dungeonGenIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
                tasks.Insert(dungeonGenIndex + 1, new PassLegacy("Corrupt Tower", GenerateCorruptTower));
            }

            if (WorldGen.crimson || WorldGen.drunkWorldGen)
            {
                int dungeonGenIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
                tasks.Insert(dungeonGenIndex + 1, new PassLegacy("Crimson House", GenerateCrimsonHouse));
            }
        }
    }
}
