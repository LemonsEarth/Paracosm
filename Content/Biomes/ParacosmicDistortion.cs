using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Paracosm.Common.Systems;
using Terraria.Graphics.Effects;


namespace Paracosm.Content.Biomes
{
    public class ParacosmicDistortion : ModBiome
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/SeveredSpace");
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return (player.ZoneRockLayerHeight) && ModContent.GetInstance<BiomeTileCounts>().parastoneCount >= 100;
        }
    }

    public class ParacosmicDistortionPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.InModBiome<ParacosmicDistortion>())
            {
                if (!Filters.Scene["DivineSeekerShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(152, 152, 255));
                }
            }
            else
            {
                if (Filters.Scene["DivineSeekerShader"].IsActive())
                {
                    Filters.Scene.Deactivate("DivineSeekerShader");
                }
            }
        }
    }
}
