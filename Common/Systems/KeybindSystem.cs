using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind ChampionsCrown;
        public static ModKeybind VortexControl;
        public static ModKeybind WanderersVeil;

        public override void Load()
        {
            ChampionsCrown = KeybindLoader.RegisterKeybind(Mod, "ChampionsCrown", "U");
            VortexControl = KeybindLoader.RegisterKeybind(Mod, "VortexControl", "Z");
            WanderersVeil = KeybindLoader.RegisterKeybind(Mod, "WanderersVeil", "V");
        }

        public override void Unload()
        {
            ChampionsCrown = null;
            VortexControl = null;
            WanderersVeil = null;
        }
    }
}
