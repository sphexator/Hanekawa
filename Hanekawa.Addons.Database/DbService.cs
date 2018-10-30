using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Addons.Database.Tables.Administration;
using Hanekawa.Addons.Database.Tables.Audio;
using Hanekawa.Addons.Database.Tables.BoardConfig;
using Hanekawa.Addons.Database.Tables.BotGame;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Database.Tables.Moderation;
using Hanekawa.Addons.Database.Tables.Profile;
using Hanekawa.Addons.Database.Tables.Stats;
using Hanekawa.Addons.Database.Tables.Stores;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Addons.Database
{
    public class DbService : DbContext
    {
        public DbService()
        {}

        public DbService(DbContextOptions options) : base(options)
        {}

        // Account
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountGlobal> AccountGlobals { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<InventoryGlobal> InventoryGlobals { get; set; }
        public virtual DbSet<LevelReward> LevelRewards { get; set; }
        public virtual DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        public virtual DbSet<Shop> Shops { get; set; }
        public virtual DbSet<StoreGlobal> StoreGlobals { get; set; }
        public virtual DbSet<EventPayout> EventPayouts { get; set; }
        public virtual DbSet<Item> Items { get; set; }

        // Stats
        public virtual DbSet<BanStat> BanStats { get; set; }
        //public virtual DbSet<BotUsageStat> BotUsageStats { get; set; }
        //public virtual DbSet<EmoteStat> EmoteStats { get; set; }
        public virtual DbSet<JoinStat> JoinStats { get; set; }
        public virtual DbSet<MessageStat> MessageStats { get; set; }
        public virtual DbSet<MuteStat> MuteStats { get; set; }
        public virtual DbSet<WarnStat> WarnStats { get; set; }

        // Achievements
        public virtual DbSet<AchievementMeta> Achievements { get; set; }
        public virtual DbSet<AchievementName> AchievementNames { get; set; }
        public virtual DbSet<AchievementTracker> AchievementTrackers { get; set; }
        public virtual DbSet<AchievementUnlock> AchievementUnlocks { get; set; }
        public virtual DbSet<AchievementDifficulty> AchievementDifficulties { get; set; }
        public virtual DbSet<AchievementType> AchievementTypes { get; set; }

        // Administration
        public virtual DbSet<Blacklist> Blacklists { get; set; }
        public virtual DbSet<EventSchedule> EventSchedules { get; set; }
        public virtual DbSet<WhitelistDesign> WhitelistDesigns { get; set; }
        public virtual DbSet<WhitelistEvent> WhitelistEvents { get; set; }

        //Clubs
        public virtual DbSet<ClubInfo> ClubInfos { get; set; }
        public virtual DbSet<ClubPlayer> ClubPlayers { get; set; }
        public virtual DbSet<ClubBlacklist> ClubBlacklists { get; set; }

        //Bot Game
        public virtual DbSet<GameEnemy> GameEnemies { get; set; }
        public virtual DbSet<GameClass> GameClasses { get; set; }
        public virtual DbSet<GameConfig> GameConfigs { get; set; }

        //Config
        public virtual DbSet<GuildConfig> GuildConfigs { get; set; }
        public virtual DbSet<GuildInfo> GuildInfos { get; set; }
        public virtual DbSet<GuildInfoLink> GuildInfoLinks { get; set; }
        public virtual DbSet<NudeServiceChannel> NudeServiceChannels { get; set; }
        public virtual DbSet<UrlFilter> UrlFilters { get; set; }
        public virtual DbSet<SingleNudeServiceChannel> SingleNudeServiceChannels { get; set; }
        public virtual DbSet<LootChannel> LootChannels { get; set; }
        public virtual DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public virtual DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        public virtual DbSet<Board> Boards { get; set; }

        //Hunger Game
        public virtual DbSet<HungerGameConfig> HungerGameConfigs { get; set; }
        public virtual DbSet<HungerGameDefault> HungerGameDefaults { get; set; }
        public virtual DbSet<HungerGameLive> HungerGameLives { get; set; }

        //Moderation
        public virtual DbSet<ModLog> ModLogs { get; set; }
        public virtual DbSet<MuteTimer> MuteTimers { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Suggestion> Suggestions { get; set; }
        public virtual DbSet<QuestionAndAnswer> QuestionAndAnswers { get; set; }
        public virtual DbSet<Warn> Warns { get; set; }
        public virtual DbSet<WarnMsgLog> WarnMsgLogs { get; set; }
        
        //Profiles
        public virtual DbSet<Background> Backgrounds { get; set; }
        public virtual DbSet<ProfileConfig> ProfileConfigs { get; set; }

        //Audio
        public virtual DbSet<Playlist> Playlists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            Config.ConnectionString = "Server=localhost;Database=yamato_test;User=root;Password=12345;";
#endif
            if (!optionsBuilder.IsConfigured)
                optionsBuilder
                .UseMySql(Config.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account
            modelBuilder.Entity<Account>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(c => c.Credit).HasMaxLength(999);
                x.Property(c => c.CreditSpecial).HasMaxLength(999);
                x.Property(c => c.Exp).HasMaxLength(999);
                x.Property(c => c.TotalExp).HasMaxLength(999);
                x.Property(c => c.Level).HasMaxLength(999);
                x.Property(c => c.Sessions).HasMaxLength(999);
                x.Property(c => c.GameKillAmount).HasMaxLength(999);
                x.Property(c => c.Rep).HasMaxLength(999);
                x.Property(c => c.StarGiven).HasMaxLength(999);
                x.Property(c => c.StarReceived).HasMaxLength(999);
                x.Property(c => c.StatMessages).HasMaxLength(999);
                x.Property(c => c.MvpCounter).HasMaxLength(999);
                x.Property(c => c.Class).HasDefaultValue(1);
            });
            modelBuilder.Entity<AccountGlobal>(x =>
            {
                x.HasKey(e => e.UserId);
                x.Property(c => c.Level).HasMaxLength(999);
                x.Property(c => c.Exp).HasMaxLength(999);
                x.Property(c => c.Rep).HasMaxLength(999);
                x.Property(c => c.TotalExp).HasMaxLength(999);
            });
            modelBuilder.Entity<LevelReward>(x => { x.HasKey(e => new { e.GuildId, e.Level }); });
            modelBuilder.Entity<LevelExpEvent>(x => x.HasKey(e => e.GuildId));
            modelBuilder.Entity<Shop>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ItemId});
            });
            modelBuilder.Entity<StoreGlobal>(x => x.HasKey(e => e.ItemId));
            modelBuilder.Entity<Inventory>(x => { x.HasKey(e => new { e.GuildId, e.UserId, e.ItemId }); });
            modelBuilder.Entity<InventoryGlobal>(x => { x.HasKey(e => new{e.UserId, e.ItemId}); });
            modelBuilder.Entity<EventPayout>(x => { x.HasKey(e => new { e.GuildId, e.UserId }); });
            modelBuilder.Entity<Item>(x =>
            {
                x.HasKey(e => e.ItemId);
                x.Property(e => e.ItemId).ValueGeneratedOnAdd();
            });

            // Stats
            modelBuilder.Entity<BanStat>(x => x.HasKey(e => new { e.GuildId, e.UserId }));
            modelBuilder.Entity<JoinStat>(x => x.HasKey(e => e.GuildId));
            modelBuilder.Entity<MessageStat>(x => x.HasKey(e => e.GuildId));
            modelBuilder.Entity<MuteStat>(x => x.HasKey(e => new { e.GuildId, e.UserId }));
            modelBuilder.Entity<WarnStat>(x => x.HasKey(e => new { e.GuildId, e.UserId }));

            // Achievement
            modelBuilder.Entity<AchievementMeta>(x =>
            {
                x.HasKey(e => e.AchievementId);
                x.Property(e => e.AchievementId).ValueGeneratedOnAdd();
                x.HasOne(p => p.AchievementName).WithMany();
                x.HasOne(p => p.AchievementDifficulty).WithMany();
                x.HasOne(p => p.AchievementType).WithMany();
            });
            modelBuilder.Entity<AchievementName>(x =>
            {
                x.HasKey(e => e.AchievementNameId);
                x.Property(e => e.AchievementNameId).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<AchievementTracker>(x => x.HasKey(e => new {e.Type, e.UserId}));
            modelBuilder.Entity<AchievementUnlock>(x =>
            {
                x.HasKey(e => new {e.AchievementId, e.UserId});
                x.HasOne(p => p.Achievement).WithMany();
            });
            modelBuilder.Entity<AchievementType>(x =>
            {
                x.HasKey(e => e.TypeId);
                x.Property(e => e.TypeId).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<AchievementDifficulty>(x =>
            {
                x.HasKey(e => e.DifficultyId);
                x.Property(e => e.DifficultyId).ValueGeneratedOnAdd();
            });

            // Administration
            modelBuilder.Entity<Blacklist>(x => x.HasKey(e => e.GuildId));
            modelBuilder.Entity<EventSchedule>(x => x.HasKey(e => new { e.Id, e.GuildId }));
            modelBuilder.Entity<WhitelistDesign>(x => x.HasKey(e => new { e.GuildId, e.UserId }));
            modelBuilder.Entity<WhitelistEvent>(x => x.HasKey(e => new { e.GuildId, e.UserId }));

            // Clubs
            modelBuilder.Entity<ClubInfo>(x =>
            {
                x.HasKey(e => new { e.Id, e.GuildId, e.Leader });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ClubPlayer>(x =>
            {
                x.HasKey(e => new { e.Id, e.ClubId, e.GuildId });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ClubBlacklist>(x =>
            {
                x.HasKey(e => new { e.ClubId, e.GuildId, e.BlackListUser });
            });

            // Bot Game
            modelBuilder.Entity<GameEnemy>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.Rare).HasDefaultValue(false);
                x.Property(e => e.Elite).HasDefaultValue(false);
            });
            modelBuilder.Entity<GameClass>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GameConfig>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.DefaultDamage).HasDefaultValue(10);
                x.Property(e => e.DefaultHealth).HasDefaultValue(10);
            });

            // Config
            modelBuilder.Entity<GuildInfo>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<GuildConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Premium).HasDefaultValue(false);
                x.Property(e => e.EmoteCurrency).HasDefaultValue(false);
                x.Property(e => e.SpecialEmoteCurrency).HasDefaultValue(false);
            });
            modelBuilder.Entity<GuildInfoLink>(x => x.HasKey(e => e.GuildId));
            modelBuilder.Entity<IgnoreChannel>(x => x.HasKey(e => new { e.GuildId, e.ChannelId }));
            modelBuilder.Entity<Board>(x => x.HasKey(e => new { e.GuildId, e.MessageId }));
            modelBuilder.Entity<WelcomeBanner>(x => x.HasKey(e => new { e.GuildId, e.Id }));
            modelBuilder.Entity<LootChannel>(x => { x.HasKey(e => new { e.GuildId, e.ChannelId }); });
            modelBuilder.Entity<NudeServiceChannel>(x => { x.HasKey(e => new { e.GuildId, e.ChannelId }); });
            modelBuilder.Entity<SingleNudeServiceChannel>(x => { x.HasKey(e => new {e.GuildId, e.ChannelId}); });
            modelBuilder.Entity<UrlFilter>(x => { x.HasKey(e => new {e.GuildId, e.ChannelId}); });
            // Hunger Game
            modelBuilder.Entity<HungerGameConfig>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<HungerGameDefault>(x => { x.HasKey(e => e.UserId); });
            modelBuilder.Entity<HungerGameLive>(x => { x.HasKey(e => e.UserId); });

            // Moderation
            modelBuilder.Entity<ModLog>(x => { x.HasKey(e => new { e.Id, e.GuildId }); });
            modelBuilder.Entity<MuteTimer>(x => { x.HasKey(e => new { e.UserId, e.GuildId }); });
            modelBuilder.Entity<Suggestion>(x => { x.HasKey(e => new { e.Id, e.GuildId }); });
            modelBuilder.Entity<QuestionAndAnswer>(x => { x.HasKey(e => new { e.Id, e.GuildId }); });
            modelBuilder.Entity<Report>(x => { x.HasKey(e => new { e.Id, e.GuildId }); });
            modelBuilder.Entity<Warn>(x => { x.HasKey(e => new { e.GuildId, e.Id }); });
            modelBuilder.Entity<WarnMsgLog>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            // Profiles
            modelBuilder.Entity<Background>(x => x.HasKey(e => e.Id));
            modelBuilder.Entity<ProfileConfig>(x => x.HasKey(e => e.Id));

            // Audio
            modelBuilder.Entity<Playlist>(x => x.HasKey(e => new {e.GuildId, e.Id}));
        }
    }
}
