using Microsoft.Xna.Framework;
using Paracosm.Common.Systems;
using Terraria;
using Terraria.ModLoader;


namespace Paracosm.Content.Biomes
{
    public class ParacosmicDistortion : ModBiome
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Audio/Music/AnotherSamePlace");
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => Color.Purple;
        public override string MapBackground => BackgroundPath;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return (player.ZoneRockLayerHeight) && ModContent.GetInstance<BiomeTileCounts>().parastoneCount >= 100;
        }
    }
}
