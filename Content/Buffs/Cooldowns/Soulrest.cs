﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Buffs.Cooldowns
{
    public class Soulrest : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}
