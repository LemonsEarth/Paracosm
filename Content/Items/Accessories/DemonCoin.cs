using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;
using Terraria.DataStructures;
using System;
using Terraria.WorldBuilding;

namespace Paracosm.Content.Items.Accessories
{
    public class DemonCoin : ModItem
    {
        static float damageBoost = 0;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs();

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 999;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DemonCoinPlayer>().demonCoin = true;
        }

        public class DemonCoinPlayer : ModPlayer
        {
            public bool demonCoin = false;

            public override void ResetEffects()
            {
                demonCoin = false;
            }

            public override void PostUpdateEquips()
            {
                if (demonCoin == false)
                {
                    return;
                }

                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);

                Player.GetDamage(DamageClass.Generic) += damageBoost;
            }

            public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
            {
                if (demonCoin == false)
                {
                    return;
                }
                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);
                modifiers.FinalDamage *= 1f + (1f - damageBoost);
            }

            public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
            {
                if (demonCoin == false)
                {
                    return;
                }
                damageBoost = ((float)Player.statLife / (float)Player.statLifeMax2);
                modifiers.FinalDamage *= 1f + (1f - damageBoost);
            }
        }
    }

    public class DemonCoinNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.VoodooDemon:
                    npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<DemonCoin>(), 5, 4));
                    break;
                case NPCID.Demon:
                    npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<DemonCoin>(), 20, 10));
                    break;
                case NPCID.Tim:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), 1, 1, 3));
                    break;
                case NPCID.WallofFlesh or NPCID.BloodNautilus:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), minimumDropped: 3, maximumDropped: 6));
                    break;
                case NPCID.Mimic or NPCID.GoblinShark:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>()));
                    break;
                case NPCID.RuneWizard:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), 1, 3, 5));
                    break;
                case NPCID.BigMimicCorruption or NPCID.BigMimicCrimson or NPCID.BigMimicHallow or NPCID.Moth or NPCID.SandElemental or NPCID.IceGolem or NPCID.BloodEelHead or NPCID.RainbowSlime or NPCID.GoblinSummoner or NPCID.PirateShip:
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DemonCoin>(), minimumDropped: 2, maximumDropped: 4));
                    break;
            }
        }
    }
}
