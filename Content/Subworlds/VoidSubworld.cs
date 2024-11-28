using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Paracosm.Content.Subworlds
{
    public class VoidSubworld : Subworld
    {
        public override int Width => 1000;
        public override int Height => 2000;
        public override bool ShouldSave => true;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new VoidGenPass()
        };
    }
}
