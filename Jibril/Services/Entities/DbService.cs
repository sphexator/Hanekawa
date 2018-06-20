using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Services.Entities
{
    public class DbService : DbContext
    {
        public DbService()
        {
        }

        public DbService(DbContextOptions<DbService> options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<ClubInfo> ClubInfos { get; set; }
        public virtual DbSet<ClubPlayer> ClubPlayers { get; set; }
        public virtual DbSet<GameEnemy> GameEnemies { get; set; }
        public virtual DbSet<GuildInfo> GuildInfos { get; set; }
        public virtual DbSet<GuildConfig> GuildConfigs { get; set; }
        public virtual DbSet<HungerGameConfig> HungerGameConfigs { get; set; }
        public virtual DbSet<HungerGameDefault> HungerGameDefaults { get; set; }
        public virtual DbSet<HungerGameLive> HungerGameLives { get; set; }
        public virtual DbSet<LevelReward> LevelRewards { get; set; }
        public virtual DbSet<ModLog> ModLogs { get; set; }
        public virtual DbSet<MuteTimer> MuteTimers { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<ShopEvent> ShopEvents { get; set; }
        public virtual DbSet<Suggestion> Suggestions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(DbInfo.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<ClubInfo>(x => { x.HasKey(e => e.Id); });
            modelBuilder.Entity<ClubPlayer>(x => { x.HasKey(e => e.ClubId); });
            modelBuilder.Entity<GameEnemy>(x => { x.HasKey(e => e.Id); });
            modelBuilder.Entity<GuildInfo>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<GuildConfig>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<HungerGameConfig>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<HungerGameDefault>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<HungerGameLive>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<LevelReward>(x => { x.HasKey(e => e.Level); });
            modelBuilder.Entity<ModLog>(x => { x.HasKey(e => e.Id); });
            modelBuilder.Entity<MuteTimer>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.HasKey(e => e.UserId);
            });
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => e.MessageId);
                x.HasKey(e => e.UserId);
            });
            modelBuilder.Entity<Shop>(x => { x.HasKey(e => e.Id); });
            modelBuilder.Entity<ShopEvent>(x => { x.HasKey(e => e.Id); });
            modelBuilder.Entity<Suggestion>(x =>
            {
                x.HasKey(e => e.UserId);
                x.HasKey(e => e.MessageId);
            });
        }
    }
}
