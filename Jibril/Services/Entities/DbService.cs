using Jibril.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Services.Entities
{
    public class DbService : DbContext
    {
        public DbService() { }

        public DbService(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountGlobal> AccountGlobals { get; set; }
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
        public virtual DbSet<Warn> Warns { get; set; }
        public virtual DbSet<WarnMsgLog> WarnMsgLogs { get; set; }
        public virtual DbSet<NudeServiceChannel> NudeServiceChannels { get; set; }
        public virtual DbSet<LootChannel> LootChannels { get; set; }
        public virtual DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public virtual DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        public virtual DbSet<Board> Boards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql("Server=localhost;Database=yamato;User=root;Password=12345;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x => { x.HasKey(e => new { e.UserId, e.GuildId }); });
            modelBuilder.Entity<AccountGlobal>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<GuildInfo>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<GuildConfig>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<HungerGameConfig>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<HungerGameDefault>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<HungerGameLive>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<LevelReward>(x => { x.HasKey(e => new { e.GuildId, e.Level}); });
            modelBuilder.Entity<MuteTimer>(x => { x.HasKey(e => new {e.UserId, e.GuildId}); });
            modelBuilder.Entity<Suggestion>(x =>
            {
                x.HasKey(e => new{e.Id, e.GuildId});
            });
            modelBuilder.Entity<LootChannel>(x => { x.HasKey(e => new {e.GuildId, e.ChannelId}); });
            modelBuilder.Entity<NudeServiceChannel>(x => { x.HasKey(e => new {e.GuildId, e.ChannelId}); });
            modelBuilder.Entity<Inventory>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<Warn>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Id});
            });
            modelBuilder.Entity<WarnMsgLog>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => new{e.Id, e.GuildId});
            });
            modelBuilder.Entity<Shop>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ShopEvent>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ModLog>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
            modelBuilder.Entity<ClubInfo>(x =>
            {
                x.HasKey(e => e.Id);
            });
            modelBuilder.Entity<ClubPlayer>(x => { x.HasKey(e => e.ClubId); });
            modelBuilder.Entity<GameEnemy>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<WelcomeBanner>(x => x.HasKey(e => new {e.GuildId, e.Id}));
            modelBuilder.Entity<IgnoreChannel>(x => x.HasKey(e => new {e.GuildId, e.ChannelId}));
            modelBuilder.Entity<Board>(x => x.HasKey(e => new {e.GuildId, e.MessageId}));
        }
    }
}