using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Paracosm.Common.Systems
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedDivineSeeker = false;

        public override void ClearWorld()
        {
            downedDivineSeeker = false;
        }


        public override void SaveWorldData(TagCompound tag)
        {
            if (downedDivineSeeker)
            {
                tag["downedDivineSeeker"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedDivineSeeker = tag.ContainsKey("downedDivineSeeker");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = downedDivineSeeker;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedDivineSeeker = flags[0];
        }
    }
}
