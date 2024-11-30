using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Content.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Paracosm.Content.Biomes.Void
{
    public class VoidSky : ModBiome
    {
        public override int Music => 0;
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => Color.Black;
        public override string MapBackground => BackgroundPath;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<VoidSkyBackgroundStyle>();

        public override bool IsBiomeActive(Player player)
        {
            return SubworldSystem.Current is VoidSubworld;
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

    public class VoidSkyBackgroundStyle : ModSurfaceBackgroundStyle
    {
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            
        }

        public override int ChooseFarTexture()
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Textures/Backgrounds/VoidSky_Background");
        }

        public override int ChooseMiddleTexture()
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Textures/Backgrounds/VoidSky_Background");
        }

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Textures/Backgrounds/VoidSky_Background");
        }
    }
}
