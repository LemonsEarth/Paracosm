using Microsoft.Xna.Framework;
using Paracosm.Content.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Paracosm.Content.Biomes.Void
{
    public class VoidMid : ModBiome
    {
        public override int Music => 0;
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => Color.Black;
        public override string MapBackground => BackgroundPath;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<VoidHighBackgroundStyle>();
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<VoidMidBackgroundStyle>();

        public override bool IsBiomeActive(Player player)
        {
            return SubworldSystem.Current is VoidSubworld && player.ZoneDirtLayerHeight;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive && SkyManager.Instance["Paracosm:VoidSky"] != null && IsBiomeActive(player) && !SkyManager.Instance["Paracosm:VoidSky"].IsActive())
            {
                SkyManager.Instance.Activate("Paracosm:VoidSky");
            }
            else
            {
                SkyManager.Instance.Deactivate("Paracosm:VoidSky");
            }
        }
    }
}
