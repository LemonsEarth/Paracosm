using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Walls
{
    public class ParastoneWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustID.Obsidian;

            AddMapEntry(new Color(5, 1, 66));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 3 : 1;
        }
    }
}
