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
using System.Collections.Generic;
using Paracosm.Content.NPCs.Hostile;
using Terraria.GameContent.Bestiary;


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

    public class ParacosmicDistortionPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.InModBiome<ParacosmicDistortion>())
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive() && Main.netMode != NetmodeID.Server)
                {
                    Terraria.Graphics.Effects.Filters.Scene.Activate("DivineSeekerShader").GetShader().UseColor(new Color(152, 152, 255));
                }
            }
            else
            {
                if (Terraria.Graphics.Effects.Filters.Scene["DivineSeekerShader"].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate("DivineSeekerShader");
                }
            }

        }
    }

    public class ParacosmicDistortionNPC : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.InModBiome<ParacosmicDistortion>())
            {
                maxSpawns = 10;
                spawnRate *= 2;
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<ParacosmicDistortion>())
            {
                pool.Clear();

                pool.Add(ModContent.NPCType<Wanderer>(), 0.4f);
                pool.Add(ModContent.NPCType<ParastoneRoller>(), 0.3f);
            }
        }
    }
}
