using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles;
using Terraria.DataStructures;
using Paracosm.Content.Items.Materials;

namespace Paracosm.Content.Items.Weapons.Melee
{
    public class HorizonSplitter : ModItem
    {
        int useCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 1111;
            Item.DamageType = DamageClass.Melee;
            Item.width = 100;
            Item.height = 100;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 60);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<HorizonSplitterProj>();
            Item.shootSpeed = 6;
            Item.channel = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float mousePosRel = Main.MouseWorld.X > player.Center.X ? 1 : -1;
            Projectile.NewProjectile(source, player.MountedCenter + new Vector2(-mousePosRel * 20, -20), Vector2.Zero, type, damage, knockback, ai1: 1, ai2: mousePosRel);
            return false;
        }

        public override bool MeleePrefix()
        {
            return true;
        }
    }
}
