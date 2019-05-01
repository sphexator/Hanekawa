using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Achievement;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Internal;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Database.Tables.Profile;
using Hanekawa.Database.Tables.Stores;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database
{
    public class DbService : DbContext
    {
        public DbService() { }
        public DbService(DbContextOptions options) : base(options) { }

        // Account
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountGlobal> AccountGlobals { get; set; }
        public virtual DbSet<LevelReward> LevelRewards { get; set; }
        public virtual DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        public virtual DbSet<EventPayout> EventPayouts { get; set; }

        // Stores
        public virtual DbSet<ServerStore> ServerStores { get; set; }

        // Inventory
        public virtual DbSet<Inventory> Inventories { get; set; }

        // Items
        public virtual DbSet<Item> Items { get; set; }

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
        public virtual DbSet<ClubInformation> ClubInfos { get; set; }
        public virtual DbSet<ClubUser> ClubPlayers { get; set; }
        public virtual DbSet<ClubBlacklist> ClubBlacklists { get; set; }

        //Bot Game
        public virtual DbSet<GameClass> GameClasses { get; set; }
        public virtual DbSet<GameConfig> GameConfigs { get; set; }
        public virtual DbSet<GameEnemy> GameEnemies { get; set; }

        //Config
        public virtual DbSet<GuildConfig> GuildConfigs { get; set; }
        public virtual DbSet<AdminConfig> AdminConfigs { get; set; }
        public virtual DbSet<BoardConfig> BoardConfigs { get; set; }
        public virtual DbSet<ChannelConfig> ChannelConfigs { get; set; }
        public virtual DbSet<ClubConfig> ClubConfigs { get; set; }
        public virtual DbSet<CurrencyConfig> CurrencyConfigs { get; set; }
        public virtual DbSet<LevelConfig> LevelConfigs { get; set; }
        public virtual DbSet<LoggingConfig> LoggingConfigs { get; set; }
        public virtual DbSet<SuggestionConfig> SuggestionConfigs { get; set; }
        public virtual DbSet<WelcomeConfig> WelcomeConfigs { get; set; }
        public virtual DbSet<DropConfig> DropConfigs { get; set; }

        public virtual DbSet<LootChannel> LootChannels { get; set; }
        public virtual DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public virtual DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        public virtual DbSet<Board> Boards { get; set; }
        public virtual DbSet<LevelExpReduction> LevelExpReductions { get; set; }
        public virtual DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }

        //Moderation
        public virtual DbSet<ModLog> ModLogs { get; set; }
        public virtual DbSet<MuteTimer> MuteTimers { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Suggestion> Suggestions { get; set; }
        public virtual DbSet<QuestionAndAnswer> QuestionAndAnswers { get; set; }
        public virtual DbSet<Warn> Warns { get; set; }
        public virtual DbSet<SpamIgnore> SpamIgnores { get; set; }
        public virtual DbSet<NudeServiceChannel> NudeServiceChannels { get; set; }
        public virtual DbSet<UrlFilter> UrlFilters { get; set; }
        public virtual DbSet<SingleNudeServiceChannel> SingleNudeServiceChannels { get; set; }

        //Profiles
        public virtual DbSet<Background> Backgrounds { get; set; }
        public virtual DbSet<ProfileConfig> ProfileConfigs { get; set; }

        // Internal
        public virtual DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            if(Config.ConnectionString == null) Config.ConnectionString = "Server=localhost;database=hanekawa-test2;Uid=postgres;Pwd=12345";
#endif
            if (!optionsBuilder.IsConfigured)
                optionsBuilder
                    .UseNpgsql(Config.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            InventoryBuilder(modelBuilder);
            ItemBuilder(modelBuilder);
            StoreBuilder(modelBuilder);
            AccountBuilder(modelBuilder);
            AchievementBuilder(modelBuilder);
            OwnerBuilder(modelBuilder);
            ClubBuilder(modelBuilder);
            ConfigBuilder(modelBuilder);
            GameBuilder(modelBuilder);
            AdministrationBuilder(modelBuilder);
            ProfileBuilder(modelBuilder);
            InternalBuilder(modelBuilder);
        }

        private void OwnerBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blacklist>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ResponsibleUser).HasConversion<long>();
            });
            modelBuilder.Entity<EventSchedule>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Host).HasConversion<long>();
                x.Property(e => e.DesignerClaim).HasConversion<long>();
            });
            modelBuilder.Entity<WhitelistDesign>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<WhitelistEvent>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
        }

        private void InventoryBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId, e.ItemId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
        }

        private void ItemBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Role).HasConversion<long>();
            });
        }

        private void StoreBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerStore>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId });
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.RoleId).HasConversion<long>();
            });
        }

        private void AccountBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.StatMessages).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<AccountGlobal>(x =>
            {
                x.HasKey(e => e.UserId);
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<LevelReward>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Level});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Role).HasConversion<long>();
            });
            modelBuilder.Entity<LevelExpEvent>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
            });
            modelBuilder.Entity<EventPayout>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
        }

        private void AchievementBuilder(ModelBuilder modelBuilder)
        {
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
            modelBuilder.Entity<AchievementTracker>(x =>
            {
                x.HasKey(e => new {e.Type, e.UserId});
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<AchievementUnlock>(x =>
            {
                x.HasKey(e => new {e.AchievementId, e.UserId});
                x.HasOne(p => p.Achievement).WithMany();
                x.Property(e => e.UserId).HasConversion<long>();
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
        }

        private void GameBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameClass>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GameConfig>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                ;
            });
            modelBuilder.Entity<GameEnemy>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }

        private void AdministrationBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModLog>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
                x.Property(e => e.ModId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<MuteTimer>(x =>
            {
                x.HasKey(e => new {e.UserId, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<Suggestion>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
                x.Property(e => e.ResponseUser).HasConversion<long>();
            });
            modelBuilder.Entity<QuestionAndAnswer>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
                x.Property(e => e.ResponseUser).HasConversion<long>();
            });
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
            });
            modelBuilder.Entity<Warn>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});

                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
            modelBuilder.Entity<NudeServiceChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<SingleNudeServiceChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<UrlFilter>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<SpamIgnore>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
        }

        private void ClubBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubInformation>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.LeaderId).HasConversion<long>();
                x.Property(e => e.AdMessage).HasConversion<long>();
                x.Property(e => e.Channel).HasConversion<long>();
                x.Property(e => e.Role).HasConversion<long>();
            });

            modelBuilder.Entity<ClubUser>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });

            modelBuilder.Entity<ClubBlacklist>(x =>
            {
                x.HasKey(e => new {e.ClubId, e.GuildId, e.BlackListUser});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.BlackListUser).HasConversion<long>();
                x.Property(e => e.IssuedUser).HasConversion<long>();
            });
        }

        private void ConfigBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Premium).HasDefaultValue(false);
                x.Property(E => E.EmbedColor).HasConversion<int>();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.AnimeAirChannel).HasConversion<long>();
                x.Property(e => e.EmbedColor).HasConversion<int>();
                x.Property(e => e.MusicChannel).HasConversion<long>();
                x.Property(e => e.MusicVcChannel).HasConversion<long>();
            });
            modelBuilder.Entity<AdminConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.MuteRole).HasConversion<long>();
            });
            modelBuilder.Entity<BoardConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Channel).HasConversion<long>();
            });
            modelBuilder.Entity<ChannelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.DesignChannel).HasConversion<long>();
                x.Property(e => e.EventChannel).HasConversion<long>();
                x.Property(e => e.EventSchedulerChannel).HasConversion<long>();
                x.Property(e => e.ModChannel).HasConversion<long>();
                x.Property(e => e.QuestionAndAnswerChannel).HasConversion<long>();
                x.Property(e => e.ReportChannel).HasConversion<long>();
            });
            modelBuilder.Entity<ClubConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.AdvertisementChannel).HasConversion<long>();
                x.Property(e => e.ChannelCategory).HasConversion<long>();
            });
            modelBuilder.Entity<CurrencyConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.EmoteCurrency).HasDefaultValue(false);
                x.Property(e => e.SpecialEmoteCurrency).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
            });
            modelBuilder.Entity<LoggingConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.LogAutoMod).HasConversion<long>();
                x.Property(e => e.LogAvi).HasConversion<long>();
                x.Property(e => e.LogBan).HasConversion<long>();
                x.Property(e => e.LogJoin).HasConversion<long>();
                x.Property(e => e.LogWarn).HasConversion<long>();
                x.Property(e => e.LogMsg).HasConversion<long>();
            });
            modelBuilder.Entity<SuggestionConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Channel).HasConversion<long>();
            });
            modelBuilder.Entity<WelcomeConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Channel).HasConversion<long>();
            });
            modelBuilder.Entity<DropConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
            });

            modelBuilder.Entity<IgnoreChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<Board>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.MessageId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
                x.Property(e => e.MessageId).HasConversion<long>();
            });
            modelBuilder.Entity<WelcomeBanner>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Uploader).HasConversion<long>();
            });
            modelBuilder.Entity<LootChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<LevelExpReduction>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.ChannelId).HasConversion<long>();
            });
            modelBuilder.Entity<SelfAssignAbleRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.RoleId).HasConversion<long>();
            });
        }

        private void ProfileBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfileConfig>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Background>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }

        private void InternalBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}