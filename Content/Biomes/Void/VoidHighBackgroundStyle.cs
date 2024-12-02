using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Paracosm.Content.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Paracosm.Content.Biomes.Void
{
    public class VoidHighBackgroundStyle : ModSurfaceBackgroundStyle
    {
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            
        }

        public override int ChooseFarTexture()
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/Void/VoidSky_Background");
        }

        public override int ChooseMiddleTexture()
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/Void/VoidSky_Background");
        }

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/Void/VoidSky_Background");
        }
    }
}
