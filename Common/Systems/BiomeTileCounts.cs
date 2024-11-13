using Paracosm.Content.Tiles;
using System;
using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class BiomeTileCounts : ModSystem
    {
        public int parastoneCount = 0;

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            parastoneCount = tileCounts[ModContent.TileType<ParastoneBlock>()];
        }
    }
}
