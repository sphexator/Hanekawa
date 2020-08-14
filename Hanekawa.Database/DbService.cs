using System;
using System.Collections.Generic;
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
using Hanekawa.Database.Tables.Music;
using Hanekawa.Database.Tables.Premium;
using Hanekawa.Database.Tables.Profile;
using Hanekawa.Database.Tables.Stores;
using Hanekawa.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database
{
    public class DbService : DbContext
    {
        public DbService(DbContextOptions options) : base(options) { }

        // Account
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountGlobal> AccountGlobals { get; set; }
        public DbSet<LevelReward> LevelRewards { get; set; }
        public DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        public DbSet<EventPayout> EventPayouts { get; set; }
        public DbSet<Highlight> Highlights { get; set; }

        // Stores
        public DbSet<ServerStore> ServerStores { get; set; }

        // Inventory
        public DbSet<Inventory> Inventories { get; set; }

        // Items
        public DbSet<Item> Items { get; set; }
        public DbSet<Background> Backgrounds { get; set; }

        // Achievements
        public DbSet<AchievementMeta> Achievements { get; set; }
        public DbSet<AchievementName> AchievementNames { get; set; }
        public DbSet<AchievementTracker> AchievementTrackers { get; set; }
        public DbSet<AchievementUnlock> AchievementUnlocks { get; set; }
        public DbSet<AchievementType> AchievementTypes { get; set; }

        // Administration
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<ApprovalQueue> ApprovalQueues { get; set; }

        //Clubs
        public DbSet<ClubInformation> ClubInfos { get; set; }
        public DbSet<ClubUser> ClubPlayers { get; set; }
        public DbSet<ClubBlacklist> ClubBlacklists { get; set; }

        //Bot Game
        public DbSet<GameClass> GameClasses { get; set; }
        public DbSet<GameConfig> GameConfigs { get; set; }
        public DbSet<GameEnemy> GameEnemies { get; set; }

        //Config
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<AdminConfig> AdminConfigs { get; set; }
        public DbSet<BoardConfig> BoardConfigs { get; set; }
        public DbSet<ChannelConfig> ChannelConfigs { get; set; }
        public DbSet<ClubConfig> ClubConfigs { get; set; }
        public DbSet<CurrencyConfig> CurrencyConfigs { get; set; }
        public DbSet<LevelConfig> LevelConfigs { get; set; }
        public DbSet<LoggingConfig> LoggingConfigs { get; set; }
        public DbSet<SuggestionConfig> SuggestionConfigs { get; set; }
        public DbSet<WelcomeConfig> WelcomeConfigs { get; set; }
        public DbSet<DropConfig> DropConfigs { get; set; }

        public DbSet<LootChannel> LootChannels { get; set; }
        public DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<LevelExpReduction> LevelExpReductions { get; set; }
        public DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }

        //Moderation
        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<MuteTimer> MuteTimers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<Warn> Warns { get; set; }

        // Music 
        public DbSet<MusicConfig> MusicConfigs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }

        // Premium
        public DbSet<MvpConfig> MvpConfigs { get; set; }

        // Internal
        public virtual DbSet<Log> Logs { get; set; }

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
            MusicBuilder(modelBuilder);
            PremiumBuilder(modelBuilder);
            InternalBuilder(modelBuilder);
        }

        private static void OwnerBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Blacklist>(x =>
        {
            x.HasKey(e => e.GuildId);
            x.Property(e => e.GuildId).HasConversion<long>();
            x.Property(e => e.ResponsibleUser).HasConversion<long>();
        });

        private static void InventoryBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Inventory>(x =>
        {
            x.HasKey(e => new {e.GuildId, e.UserId, e.ItemId});
            x.Property(e => e.GuildId).HasConversion<long>();
            x.Property(e => e.UserId).HasConversion<long>();
        });

        private static void ItemBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Item>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Id).ValueGeneratedOnAdd();
            x.Property(e => e.GuildId).HasConversion<long>();
            x.Property(e => e.Role).HasConversion<long>();
            x.Property(e => e.Type).HasConversion(
                v => v.ToString(),
                v => (ItemType) Enum.Parse(typeof(ItemType), v));
        });

        private static void StoreBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<ServerStore>(x =>
        {
            x.HasKey(e => new {e.GuildId, e.RoleId});
            x.Property(e => e.GuildId).HasConversion<long>();
            x.Property(e => e.RoleId).HasConversion<long>();
        });

        private static void AccountBuilder(ModelBuilder modelBuilder)
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
                x.Property(e => e.UserColor).HasConversion<int>();
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
            modelBuilder.Entity<Highlight>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.UserId).HasConversion<long>();
            });
        }

        private static void AchievementBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AchievementMeta>(x =>
            {
                x.HasKey(e => e.AchievementId);
                x.Property(e => e.AchievementId).ValueGeneratedOnAdd();
                x.HasOne(p => p.AchievementName).WithMany();
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
        }

        private static void GameBuilder(ModelBuilder modelBuilder)
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

        private static void AdministrationBuilder(ModelBuilder modelBuilder)
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
            modelBuilder.Entity<ApprovalQueue>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.Uploader).HasConversion<long>();
                x.Property(e => e.Type).HasConversion(
                v => v.ToString(),
                v => (ApprovalQueueType)Enum.Parse(typeof(ApprovalQueueType), v));
            });
        }

        private static void ClubBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubInformation>(x =>
            {
                x.HasKey(e => new {e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.LeaderId).HasConversion<long>();
                x.Property(e => e.AdMessage).HasConversion<long>();
                x.Property(e => e.Channel).HasConversion<long>();
                x.Property(e => e.Role).HasConversion<long>();
            });

            modelBuilder.Entity<ClubUser>(x =>
            {
                x.HasKey(e => new {e.Id});
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

        private static void ConfigBuilder(ModelBuilder modelBuilder)
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
                x.Property(e => e.ChannelType).HasConversion(
                    v => v.ToString(),
                    v => (ChannelType) Enum.Parse(typeof(ChannelType), v));
            });
            modelBuilder.Entity<SelfAssignAbleRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId});
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.RoleId).HasConversion<long>();
            });
        }

        private static void ProfileBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Background>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Id).ValueGeneratedOnAdd();
            x.HasData(new List<Background>
            {
                new Background
                {
                    Id = 1,
                    BackgroundUrl = "https://i.imgur.com/epIb29P.png"
                },
                new Background
                {
                    Id = 2,
                    BackgroundUrl = "https://i.imgur.com/04PbzvT.png"
                },
                new Background
                {
                    Id = 3,
                    BackgroundUrl = "https://i.imgur.com/5ojmh76.png"
                },
                new Background
                {
                    Id = 4,
                    BackgroundUrl = "https://i.imgur.com/OAMpNDh.png"
                },
                new Background
                {
                    Id = 5,
                    BackgroundUrl = "https://i.imgur.com/KXO5bx5.png"
                },
                new Background
                {
                    Id = 6,
                    BackgroundUrl = "https://i.imgur.com/5h5zZ7C.png"
                }
            });
        });

        private static void MusicBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MusicConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.TextChId).HasConversion<long>();
                x.Property(e => e.VoiceChId).HasConversion<long>();
            });
            modelBuilder.Entity<Playlist>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Name});
                x.Property(e => e.GuildId).HasConversion<long>();
            });
        }

        private static void PremiumBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MvpConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.GuildId).HasConversion<long>();
                x.Property(e => e.RoleId).HasConversion<long>();
                x.Property(e => e.Day).HasConversion(
                    v => v.ToString(),
                    v => (DayOfWeek) Enum.Parse(typeof(DayOfWeek), v));
            });
        }

        private static void InternalBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Log>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}