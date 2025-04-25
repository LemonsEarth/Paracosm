using Microsoft.Xna.Framework;
using Paracosm.Common.Utils;
using Paracosm.Content.Items.Weapons.Magic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class VoidBolt : ModProjectile
    {
        ref float AITimer => ref Projectile.ai[0];
        ref float closestNPC => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 120;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = 50;
        }

        public override void AI()
        {
            if (AITimer == 0 && Main.myPlayer == Projectile.owner)
            {
                NPC npc = LemonUtils.GetClosestNPC(Projectile);
                if (npc != null && npc.active && npc.Distance(Main.MouseWorld) < 300)
                {
                    closestNPC = npc.whoAmI;
                }
            }

            if (Main.npc[(int)closestNPC] != null && Main.npc[(int)closestNPC].active && Main.npc[(int)closestNPC].CanBeChasedBy())
            {
                Projectile.velocity += Projectile.Center.DirectionTo(Main.npc[(int)closestNPC].Center) * 2f;
            }

            for (int i = 0; i < 2; i++)
            {
                var dust = Dust.NewDustDirect(Projectile.position, 2, 2, DustID.Granite, newColor: Color.Black);
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 2, 0, 2);
            AITimer++;
        }

        public override void OnKill(int timeLeft)
        {
            LemonUtils.DustCircle(Projectile.Center, 16, 5, DustID.Granite);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.type == ModContent.ItemType<VoidcoreStaff>())
            {
                VoidcoreStaff staff = player.HeldItem.ModItem as VoidcoreStaff;
                if (staff.chargeAmount < 100 && player.altFunctionUse == 0) staff.chargeAmount += 5;
            }
        }
    }
}
