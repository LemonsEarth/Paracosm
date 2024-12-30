using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using Paracosm.Common.Utils;
using Paracosm.Content.Biomes.Void;
using Paracosm.Content.Items.Materials;
using Paracosm.Content.Projectiles.Hostile;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Paracosm.Content.NPCs.Hostile.Void
{
    public class VoidPlayer : ModNPC
    {
        ref float AITimer => ref NPC.ai[0];
        ref float AttackTimer => ref NPC.ai[1];
        ref float Class => ref NPC.ai[2];

        Vector2 targetPosition = Vector2.Zero;
        Player player;

        enum Classes
        {
            Melee,
            Ranged,
            Magic,
            Summon
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 6000;
            NPC.defense = 100;
            NPC.damage = 30;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.value = 6000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 5;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<VoidLow>().Type };
            NPC.noTileCollide = true;
            NPC.knockBackResist = 1f;
            NPC.noGravity = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>()
                {
                    new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                });
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Class = Main.rand.Next(0, 4);
            }
            NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255)
            {
                NPC.TargetClosest(false);
            }
            player = Main.player[NPC.target];

            if (AITimer == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, NPC.Center);
            }

            if (!NPC.Center.DirectionTo(player.Center).HasNaNs())
            {
                NPC.spriteDirection = -Math.Sign(NPC.Center.DirectionTo(player.Center).X);
            }

            switch (Class)
            {
                case (int)Classes.Melee:
                    Melee();
                    Dust dust = Dust.NewDustDirect(NPC.Center - Vector2.UnitY * 9, 2, 2, DustID.GemAmber, 2f);
                    dust.noGravity = true;
                    NPC.defense = 130;
                    NPC.knockBackResist = 0f;
                    break;
                case (int)Classes.Ranged:
                    Ranged();
                    Dust dust1 = Dust.NewDustDirect(NPC.Center - Vector2.UnitY * 9, 2, 2, DustID.GemEmerald, 2f);
                    dust1.noGravity = true;
                    NPC.defense = 100;
                    break;
                case (int)Classes.Magic:
                    Magic();
                    Dust dust2 = Dust.NewDustDirect(NPC.Center - Vector2.UnitY * 9, 2, 2, DustID.GemAmethyst, 2f);
                    dust2.noGravity = true;
                    NPC.defense = 80;
                    break;
                case (int)Classes.Summon:
                    Summon();
                    Dust dust3 = Dust.NewDustDirect(NPC.Center - Vector2.UnitY * 9, 2, 2, DustID.BlueTorch, 2f);
                    dust3.noGravity = true;
                    NPC.defense = 60;
                    break;
            }
            if (AttackTimer > 0)
            {
                AttackTimer--;
            }

            Lighting.AddLight(NPC.Center, 10, 10, 10);
            AITimer++;
        }

        const int MELEE_DASH_CD = 180;
        const int MELEE_POS_TIME = 60;
        void Melee()
        {
            switch (AttackTimer)
            {
                case > MELEE_POS_TIME:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center) * 5;
                    break;
                case MELEE_POS_TIME:
                    targetPosition = player.Center;
                    break;
                case > 30:
                    NPC.velocity = Vector2.Zero;
                    break;
                case 30:
                    if (targetPosition != Vector2.Zero)
                    {
                        NPC.velocity = NPC.Center.DirectionTo(targetPosition) * 30;
                    }
                    break;
                case 0:
                    AttackTimer = MELEE_DASH_CD + 1;
                    return;
            }
        }

        const int RANGED_SHOOT_CD = 120;
        const int RANGED_POS_TIME = 30;
        void Ranged()
        {
            switch (AttackTimer)
            {
                case > RANGED_POS_TIME:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center);
                    break;
                case RANGED_POS_TIME:
                    targetPosition = player.Center;
                    break;
                case > 0:
                    NPC.velocity = Vector2.Zero;
                    break;
                case 0:
                    AttackTimer = RANGED_SHOOT_CD + 1;
                    if (targetPosition == Vector2.Zero)
                    {
                        return;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var bullet = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPosition) * 10, ProjectileID.SniperBullet, NPC.damage, 1f);
                        bullet.tileCollide = false;
                        NetMessage.SendData(MessageID.SyncNPC, number: bullet.whoAmI);
                    }
                    return;
            }
        }

        const int MAGIC_SHOOT_CD = 120;
        const int MAGIC_POS_TIME = 30;
        void Magic()
        {
            switch (AttackTimer)
            {
                case > MAGIC_POS_TIME:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center) * 3;
                    break;
                case MAGIC_POS_TIME:
                    targetPosition = player.Center;
                    break;
                case > 0:
                    NPC.velocity = Vector2.Zero;
                    break;
                case 0:
                    AttackTimer = MAGIC_SHOOT_CD + 1;
                    if (targetPosition == Vector2.Zero)
                    {
                        return;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(targetPosition).RotatedBy(MathHelper.ToRadians(18 * i)) * 5, ProjectileID.LostSoulHostile, NPC.damage, 1f);
                        }
                    }
                    return;
            }
        }

        const int SUMMON_CD = 600;
        const int SUMMON_POS_TIME = 120;
        void Summon()
        {
            switch (AttackTimer)
            {
                case > SUMMON_POS_TIME:
                    NPC.velocity = NPC.Center.DirectionTo(player.Center) * 4;
                    break;
                case SUMMON_POS_TIME:
                    targetPosition = player.Center;
                    break;
                case > 0:
                    NPC.velocity = Vector2.Zero;
                    break;
                case 0:
                    AttackTimer = SUMMON_CD + 1;
                    SpawnMinions();
                    return;
            }
        }

        void SpawnMinions()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = NPC.Center + (-Vector2.UnitY * 50).RotatedBy(MathHelper.ToRadians(i * 120));
                LemonUtils.DustCircle(pos, 16, 10, DustID.GemDiamond, 1.2f);
                NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), pos, Main.rand.NextFromList(83, 84, 179), NPC.whoAmI);
                npc.lifeMax = 12000;
                npc.life = 12000;
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
                }
            }         
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome<VoidLow>() ? 0.06f : 0;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidEssence>(), minimumDropped: 3, maximumDropped: 7));
        }

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
    }
}
