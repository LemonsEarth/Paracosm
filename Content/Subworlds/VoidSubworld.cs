
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Paracosm.Content.Subworlds
{
    public class VoidSubworld : Subworld
    {
        public override int Width => 1000;
        public override int Height => 3000;
        public override bool ShouldSave => true;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new VoidGenPass(),
            new VoidTerrainGenPass(),
            new VoidStructuresGenPass(),
            new VoidChestGenPass(),
            new VoidPaintBlackGenPass()
        };

        public override void OnEnter()
        {
            SubworldSystem.hideUnderworld = true;
        }

        public override void Update()
        {
            SubworldSystem.hideUnderworld = true;
        }
    }
}
