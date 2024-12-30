using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;

namespace Paracosm.Content.Items.Rarities
{
    public class ParacosmRarity
    {
        public static int Orange => ModContent.RarityType<RarityOrange>();
        public static int LightBlue => ModContent.RarityType<RarityLightBlue>();
        public static int LightGreen => ModContent.RarityType<RarityLightGreen>();
        public static int PinkPurple => ModContent.RarityType<RarityPinkPurple>();
        public static int DarkGray => ModContent.RarityType<DarkGray>();
    }

    public class RarityOrange : ModRarity
    {
        public override Color RarityColor => new Color(252, 161, 3);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }

    public class RarityLightBlue : ModRarity
    {
        public override Color RarityColor => new Color(3, 232, 255);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }

    public class RarityLightGreen : ModRarity
    {
        public override Color RarityColor => new Color(5, 242, 175);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }

    public class RarityPinkPurple : ModRarity
    {
        public override Color RarityColor => new Color(210, 120, 255);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }

    public class DarkGray : ModRarity
    {
        public override Color RarityColor => new Color(50, 50, 50);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}
