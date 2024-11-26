using Paracosm.Content.Items.Armor.Celestial;
using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind ChampionsCrown;
        public static ModKeybind VortexControl;

        public override void Load()
        {
            ChampionsCrown = KeybindLoader.RegisterKeybind(Mod, "Champion's Crown", "U");
            VortexControl = KeybindLoader.RegisterKeybind(Mod, "Vortex Control", "Z");
        }

        public override void Unload()
        {
            ChampionsCrown = null;
            VortexControl = null;
        }
    }
}
