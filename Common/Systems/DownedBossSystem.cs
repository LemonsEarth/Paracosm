using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Paracosm.Common.Systems
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedDivineSeeker = false;
        public static bool downedInfectedRevenant = false;
        public static bool downedSolarChampion = false;

        public override void ClearWorld()
        {
            downedDivineSeeker = false;
            downedInfectedRevenant = false;
            downedSolarChampion = false;
        }


        public override void SaveWorldData(TagCompound tag)
        {
            if (downedDivineSeeker)
            {
                tag["downedDivineSeeker"] = true;
            }
            if (downedInfectedRevenant)
            {
                tag["downedInfectedRevenant"] = true;
            }
            if (downedSolarChampion)
            {
                tag["downedSolarChampion"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedDivineSeeker = tag.ContainsKey("downedDivineSeeker");
            downedInfectedRevenant = tag.ContainsKey("downedInfectedRevenant");
            downedSolarChampion = tag.ContainsKey("downedSolarChampion");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = downedDivineSeeker;
            flags[1] = downedInfectedRevenant;
            flags[2] = downedSolarChampion;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedDivineSeeker = flags[0];
            downedInfectedRevenant = flags[1];
            downedSolarChampion = flags[2];
        }
    }
}
