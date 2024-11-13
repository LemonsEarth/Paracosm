using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Paracosm
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class Paracosm : Mod
    {
        public override void Load()
        {
            Asset<Effect> filterShader = this.Assets.Request<Effect>("Content/Shaders/DivineSeekerShader");
            Filters.Scene["DivineSeekerShader"] = new Filter(new ScreenShaderData(filterShader, "ScreenTint"), EffectPriority.Medium);
        }
    }
}
