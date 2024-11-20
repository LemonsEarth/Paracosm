using Microsoft.Xna.Framework;
using Paracosm.Content.Buffs;
using Paracosm.Content.Projectiles.Hostile;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.Projectiles.Friendly
{
    public class SolarExplosionMagic : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SolarWhipSwordExplosion);
            Projectile.aiStyle = ProjAIStyleID.SolarEffect;
            AIType = ProjectileID.SolarWhipSwordExplosion;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SolarBurn>(), 120);
        }
    }
}
