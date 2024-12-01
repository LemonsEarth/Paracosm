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

namespace Paracosm.Content.Subworlds
{
    public class VoidGenPass : GenPass
    {
        public VoidGenPass() : base("Void", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "The Void";
            Main.worldSurface = Main.maxTilesY - 42;
            Main.rockLayer = Main.maxTilesY;
        }
    }

    public class VoidStructureGenPass : GenPass
    {
        public VoidStructureGenPass() : base("VoidStructures", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "???";
            GenerateAbandonedArmory(progress, configuration);
        }

        private void GenerateAbandonedArmory(GenerationProgress progress, GameConfiguration config)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int index = 0; index < 5; index++)
                {
                    bool successfulGen = false;
                    Point16 point = Point16.Zero;
                    while (!successfulGen)
                    {
                        Point16 dims = Point16.Zero;
                        Generator.GetMultistructureDimensions("Content/Structures/AbandonedArmory", ModLoader.GetMod("Paracosm"), index, ref dims);
                        int x = WorldGen.genRand.Next(dims.X, 800 - dims.X);
                        int y = WorldGen.genRand.Next((Main.maxTilesY - 200) / 3, 3000 - 200);

                        point = new Point16(x, y);
                        if (!WorldGenSystem.CheckForTiles(x, y, dims.X, dims.Y, TileID.Stone))
                        {
                            successfulGen = true;
                        }
                    }

                    if (successfulGen && point != Point16.Zero)
                    {
                        Generator.GenerateMultistructureSpecific("Content/Structures/AbandonedArmory", point, ModLoader.GetMod("Paracosm"), index);
                    }
                }
            }
        }
    }
}
