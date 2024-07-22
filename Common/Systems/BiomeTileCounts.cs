using Paracosm.Content.Bosses;
using Paracosm.Content.Items.Consumables;
using Paracosm.Content.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
