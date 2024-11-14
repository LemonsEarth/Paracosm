using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind SolarExplosion;

        public override void Load()
        {
            SolarExplosion = KeybindLoader.RegisterKeybind(Mod, "SolarExplosion", "U");
        }

        public override void Unload()
        {
            SolarExplosion = null;
        }
    }
}
