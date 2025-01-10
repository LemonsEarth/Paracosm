using Microsoft.Xna.Framework;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Items.Rarities;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Projectiles.HeldProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Items.Weapons.Magic
{
    public class DevourerRift : ModItem
    {

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Magic;
            Item.width = 58;
            Item.height = 48;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ParacosmRarity.DarkGray;
            Item.UseSound = SoundID.Item84;
            Item.autoReuse = true;
            Item.mana = 15;
            Item.shoot = ModContent.ProjectileType<VoidVortexFriendly>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center;
            velocity = Vector2.Zero;
        }
    }
}