using Paracosm.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Paracosm.Content.Projectiles.Friendly;
using Paracosm.Content.Buffs.Cooldowns;

namespace Paracosm.Common.Globals
{
    public class ParacosmGlobalItem : GlobalItem
    {
        public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            if (player.GetModPlayer<ParacosmPlayer>().nebulousPower)
            {
                ascentWhenFalling = 3f;
                ascentWhenRising = 0.5f;
                maxCanAscendMultiplier = 1f;
                maxAscentMultiplier = 3.5f;
                constantAscend = 0.135f;
            }
        }

        public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
        {
            if (player.GetModPlayer<ParacosmPlayer>().nebulousPower)
            {
                speed = 15f;
                acceleration = 1f;
            }
        }

        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (player.GetModPlayer<ParacosmPlayer>().secondHand)
            {
                if (item.damage > 0 && item.CountsAsClass(DamageClass.Melee))
                {
                    scale += 1f;
                }
            }
        }

        public override void GrabRange(Item item, Player player, ref int grabRange)
        {
            if (player.GetModPlayer<ParacosmPlayer>().nebulousPower)
            {
                if (item.type == ItemID.NebulaPickup1 || item.type == ItemID.NebulaPickup2 || item.type == ItemID.NebulaPickup3)
                {
                    grabRange += 800;
                }
            }
        }

        public override bool? UseItem(Item item, Player player)
        {
            if (!player.HasBuff(ModContent.BuffType<StardustTailCD>()) && player.GetModPlayer<ParacosmPlayer>().stardustTailSet)
            {
                if (item.damage > 0)
                {
                    if (Main.myPlayer == player.whoAmI)
                    {
                        Vector2 mouseDir = player.Center.DirectionTo(Main.MouseWorld);
                        int damage = (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(200);
                        Projectile.NewProjectile(player.GetSource_FromAI(), player.Center, mouseDir.SafeNormalize(Vector2.Zero) * 8, ModContent.ProjectileType<StardustTailProj>(), damage, 1);
                        if (player.GetModPlayer<ParacosmPlayer>().nebulousEnergy)
                        {
                            player.AddBuff(ModContent.BuffType<StardustTailCD>(), 30);
                        }
                        else
                        {
                            player.AddBuff(ModContent.BuffType<StardustTailCD>(), 60);
                        }
                    }
                }
            }

            return null;
        }
    }
}
