using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons
{
    public class Judgement : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 134;
            Item.rare = ParacosmRarity.DarkGray;
            Item.value = Item.sellPrice(0, 30);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.autoReuse = true;
            Item.damage = 500;
            Item.knockBack = 6;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Generic;
            Item.shootSpeed = 0;
            Item.shoot = ModContent.ProjectileType<JudgementProj>();
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return true;
        }
    }
}
