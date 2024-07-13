using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Weapons;
using Paracosm.Content.Projectiles;
using Terraria.Localization;

namespace Paracosm.Content.Items.Accessories
{
    public class ParashardSigil : ModItem
    {
        static readonly int damageBoost = 10;
        static readonly int critBoost = 10;
        static readonly int defenseBoost = 10;
        static readonly int lifeRegenBoost = 2;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(damageBoost, critBoost, defenseBoost, lifeRegenBoost);

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = 50000;
            Item.rare = ItemRarityID.Expert;
            Item.defense = defenseBoost;
            Item.lifeRegen = lifeRegenBoost;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += damageBoost / 100;
            player.GetCritChance(DamageClass.Generic) += critBoost;
            player.GetModPlayer<ParashardSigilPlayer>().parashardSigil = true;
        }
    }

    public class ParashardSigilPlayer : ModPlayer
    {
        float hitTimer = 120;
        bool activeTimer = false;
        public bool parashardSigil = false;

        public override void ResetEffects()
        {
            parashardSigil = false;
        }

        public override void PostUpdateEquips()
        {
            if (!parashardSigil)
            {
                return;
            }

            if (activeTimer == true)
            {
                hitTimer--;
            }
            if (hitTimer == 0)
            {
                hitTimer = 120;
                activeTimer = false;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!parashardSigil)
            {
                return;
            }
            if (activeTimer == false && hitTimer > 0)
            {
                activeTimer = true;
                for (int i = 0; i < 5; i++)
                {
                    Projectile.NewProjectile(Item.GetSource_None(), Player.Center, Main.rand.NextVector2Circular(1, 1).SafeNormalize(Vector2.Zero) * 10, ModContent.ProjectileType<HomingBlueFire>(), 100 + info.Damage, info.Knockback);
                }
            }      
        }
    }
}
