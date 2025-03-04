﻿using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Subworlds;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Consumables
{
    public class DarkMagicMirror : ModItem
    {
        bool doAnimation = false;
        float animationTimer = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 68;
            Item.maxStack = 1;
            Item.rare = ParacosmRarity.DarkGray;
            Item.useAnimation = 121;
            Item.useTime = 121;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
        }

        public override bool CanUseItem(Player player)
        {
            return !doAnimation;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item6 with { Pitch = -0.6f }, player.position);
            doAnimation = true;

            return true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation = player.Center + new Vector2(-34 * player.direction, -34 * (float)Math.Sin(MathHelper.ToRadians(animationTimer)));
        }

        public override void UpdateInventory(Player player)
        {
            if (!doAnimation)
            {
                return;
            }
            for (int i = 0; i < (animationTimer % 20); i++)
            {
                Dust dust = Dust.NewDustDirect(player.Center + new Vector2(0, -68 * (float)Math.Sin(MathHelper.ToRadians(animationTimer))), 1, 1, DustID.Granite, newColor: Color.Black, Scale: Main.rand.NextFloat(1.2f, 5.6f));
                dust.velocity = new Vector2(0, Main.rand.Next(4, 60)).RotatedByRandom(MathHelper.Pi * 2);
                dust.noGravity = true;
            }
            if (animationTimer > 120)
            {
                doAnimation = false;
                animationTimer = 0;
                if (!SubworldSystem.IsActive("VoidSubworld") && Main.myPlayer == player.whoAmI)
                {
                    SubworldSystem.Enter<VoidSubworld>();
                }
            }
            animationTimer++;
        }

        public override void AddRecipes()
        {
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.MagicMirror);
            recipe1.AddIngredient(ItemID.LunarBar, 5);
            recipe1.AddIngredient(ItemID.FragmentSolar, 5);
            recipe1.AddIngredient(ItemID.FragmentVortex, 5);
            recipe1.AddIngredient(ItemID.FragmentNebula, 5);
            recipe1.AddIngredient(ItemID.FragmentStardust, 5);
            recipe1.AddTile(TileID.LunarCraftingStation);
            recipe1.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.IceMirror);
            recipe2.AddIngredient(ItemID.LunarBar, 5);
            recipe2.AddIngredient(ItemID.FragmentSolar, 5);
            recipe2.AddIngredient(ItemID.FragmentVortex, 5);
            recipe2.AddIngredient(ItemID.FragmentNebula, 5);
            recipe2.AddIngredient(ItemID.FragmentStardust, 5);
            recipe2.AddTile(TileID.LunarCraftingStation);
            recipe2.Register();
        }
    }
}
