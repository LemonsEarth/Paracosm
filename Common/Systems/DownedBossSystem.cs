﻿using System.IO;
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
        public static bool downedVortexMothership = false;
        public static bool downedNebulaMaster = false;
        public static bool downedStardustLeviathan = false;
        public static bool downedTheNameless = false;

        public override void ClearWorld()
        {
            downedDivineSeeker = false;
            downedInfectedRevenant = false;
            downedSolarChampion = false;
            downedVortexMothership = false;
            downedNebulaMaster = false;
            downedStardustLeviathan = false;
            downedTheNameless = false;
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
            if (downedVortexMothership)
            {
                tag["downedVortexMothership"] = true;
            }
            if (downedNebulaMaster)
            {
                tag["downedNebulaMaster"] = true;
            }
            if (downedStardustLeviathan)
            {
                tag["downedStardustLeviathan"] = true;
            }
            if (downedTheNameless)
            {
                tag["downedTheNameless"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedDivineSeeker = tag.ContainsKey("downedDivineSeeker");
            downedInfectedRevenant = tag.ContainsKey("downedInfectedRevenant");
            downedSolarChampion = tag.ContainsKey("downedSolarChampion");
            downedVortexMothership = tag.ContainsKey("downedVortexMothership");
            downedNebulaMaster = tag.ContainsKey("downedNebulaMaster");
            downedStardustLeviathan = tag.ContainsKey("downedStardustLeviathan");
            downedTheNameless = tag.ContainsKey("downedTheNameless");
        }

        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = downedDivineSeeker;
            flags[1] = downedInfectedRevenant;
            flags[2] = downedSolarChampion;
            flags[3] = downedVortexMothership;
            flags[4] = downedNebulaMaster;
            flags[5] = downedStardustLeviathan;
            flags[6] = downedTheNameless;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedDivineSeeker = flags[0];
            downedInfectedRevenant = flags[1];
            downedSolarChampion = flags[2];
            downedVortexMothership = flags[3];
            downedNebulaMaster = flags[4];
            downedStardustLeviathan = flags[5];
            downedTheNameless = flags[6];
        }
    }
}
