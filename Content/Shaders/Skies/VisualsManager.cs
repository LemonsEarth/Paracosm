using Paracosm.Content.Biomes.Void;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Paracosm.Content.Shaders.Skies
{
    public class VisualsManager : ModSceneEffect
    {
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (player.InModBiome<VoidSky>() && isActive && !SkyManager.Instance["Paracosm:VoidSkySky"].IsActive())
            {
                SkyManager.Instance.Activate("Paracosm:VoidSkySky");
            }
            else
            {
                SkyManager.Instance.Deactivate("Paracosm:VoidSkySky");
            }
        }
    }
}
