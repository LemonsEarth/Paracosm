using Terraria.ModLoader;

namespace Paracosm.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind ChampionsCrown;

        public override void Load()
        {
            ChampionsCrown = KeybindLoader.RegisterKeybind(Mod, "Champion's Crown", "U");
        }

        public override void Unload()
        {
            ChampionsCrown = null;
        }
    }
}
