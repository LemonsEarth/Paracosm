using Microsoft.Xna.Framework.Graphics;
using Paracosm.Content.Shaders.Skies;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Paracosm
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class Paracosm : Mod
    {
        public static Paracosm Instance { get; private set; }
        public override void Load()
        {
            Instance = this;

            Asset<Effect> filterShader = Assets.Request<Effect>("Content/Shaders/ScreenTintShader");
            Filters.Scene["ScreenTintShader"] = new Filter(new ScreenShaderData(filterShader, "ScreenTint"), EffectPriority.Medium);

            Asset<Effect> darknessShader = Assets.Request<Effect>("Content/Shaders/DarknessShader");
            Filters.Scene["DarknessShader"] = new Filter(new ScreenShaderData(darknessShader, "Darkness"), EffectPriority.Medium);

            SkyManager.Instance["Paracosm:VoidSky"] = new VoidSkySky();
        }
    }
}
