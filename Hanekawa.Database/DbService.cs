using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Account.Achievement;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Database.Tables.Account.Profile;
using Hanekawa.Database.Tables.Account.ShipGame;
using Hanekawa.Database.Tables.Account.Stores;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Database.Tables.Advertise;
using Hanekawa.Database.Tables.AutoMessage;
using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Database.Tables.Internal;
using Hanekawa.Database.Tables.Moderation;
using Hanekawa.Database.Tables.Premium;
using Hanekawa.Database.Tables.Quote;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hanekawa.Database
{
    public class DbService : DbContext
    { 
        public DbService(DbContextOptions<DbService> options) : base(options) { }

        // Account
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountGlobal> AccountGlobals { get; set; }
        public DbSet<LevelReward> LevelRewards { get; set; }
        public DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        public DbSet<EventPayout> EventPayouts { get; set; }
        public DbSet<Highlight> Highlights { get; set; }

        // Giveaway
        public DbSet<Giveaway> Giveaways { get; set; }
        public DbSet<GiveawayParticipant> GiveawayParticipants { get; set; }
        public DbSet<GiveawayHistory> GiveawayHistories { get; set; }

        // Voice Role
        public DbSet<VoiceRoles> VoiceRoles { get; set; }

        // Stores
        public DbSet<ServerStore> ServerStores { get; set; }

        // Inventory
        public DbSet<Inventory> Inventories { get; set; }

        // Items
        public DbSet<Item> Items { get; set; }
        public DbSet<Background> Backgrounds { get; set; }

        // Achievements
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<AchievementUnlocked> AchievementUnlocks { get; set; }

        // Administration
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<ApprovalQueue> ApprovalQueues { get; set; }

        //Clubs
        public DbSet<Club> ClubInfos { get; set; }
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
        public DbSet<BoostConfig> BoostConfigs { get; set; }

        public DbSet<LootChannel> LootChannels { get; set; }
        public DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<LevelExpReduction> LevelExpReductions { get; set; }
        public DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }
        public DbSet<SelfAssignReactionRole> SelfAssignReactionRoles { get; set; }

        //Moderation
        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<MuteTimer> MuteTimers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<Warn> Warns { get; set; }

        // Premium
        public DbSet<MvpConfig> MvpConfigs { get; set; }

        // Hunger Games
        public DbSet<HungerGame> HungerGames { get; set; }
        public DbSet<HungerGameCustomChar> HungerGameCustomChars { get; set; }
        public DbSet<HungerGameDefault> HungerGameDefaults { get; set; }
        public DbSet<HungerGameHistory> HungerGameHistories { get; set; }
        public DbSet<HungerGameProfile> HungerGameProfiles { get; set; }
        public DbSet<HungerGameStatus> HungerGameStatus { get; set; }

        // Advertise
        public DbSet<DblAuth> DblAuths { get; set; }
        public DbSet<VoteLog> VoteLogs { get; set; }
        public DbSet<AutoMessage> AutoMessages { get; set; }

        // Quote
        public DbSet<Quote> Quotes { get; set; }

        // Internal
        public virtual DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseValueConverterForType<Snowflake>(
                new ValueConverter<Snowflake, ulong>(snowflake => snowflake.RawValue, 
                    @ulong => new Snowflake(@ulong)));
            modelBuilder.UseValueConverterForType<Snowflake?>(
                new ValueConverter<Snowflake?, ulong>(snowflake => snowflake.Value.RawValue, 
                    @ulong => new Snowflake(@ulong)));
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
            PremiumBuilder(modelBuilder);
            HungerGameBuilder(modelBuilder);
            InternalBuilder(modelBuilder);
            Advertise(modelBuilder);
            Giveaway(modelBuilder);

            modelBuilder.Entity<VoiceRoles>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.VoiceId });
            });
            modelBuilder.Entity<AutoMessage>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.Name });
            });

            modelBuilder.Entity<Quote>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.Key });
            });
        }

        private static void Giveaway(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Giveaway>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasMany(e => e.Participants)
                    .WithOne(e => e.Giveaway)
                    .HasForeignKey(e => e.GiveawayId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<GiveawayParticipant>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GiveawayHistory>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.Winner).HasConversion(c => c.Select(item => item.RawValue).ToArray(),
                    wops => wops.Select(item => new Snowflake(item)).ToArray());
            });
        }

        private static void Advertise(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DblAuth>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.AuthKey).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<VoteLog>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }

        private static void OwnerBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Blacklist>(x =>
        {
            x.HasKey(e => e.GuildId);
        });

        private static void InventoryBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Inventory>(x =>
        {
            x.HasKey(e => new {e.UserId});
            x.HasMany(e => e.Items).WithMany(e => e.Users);
        });
    
        private static void ItemBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Item>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Id).ValueGeneratedOnAdd();
            x.Property(e => e.ItemJson).HasColumnType("jsonb");
        });

        private static void StoreBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<ServerStore>(x =>
        {
            x.HasKey(e => new {e.GuildId, e.RoleId});
        });

        private static void AccountBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.Decay).HasDefaultValue(0);
            });
            modelBuilder.Entity<AccountGlobal>(x =>
            {
                x.HasKey(e => e.UserId);
            });
            modelBuilder.Entity<LevelReward>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Level});
                x.Property(e => e.NoDecay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelExpEvent>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<EventPayout>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
            });
            modelBuilder.Entity<Highlight>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
            });
        }

        private static void AchievementBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Achievement>(x =>
            {
                x.HasKey(e => e.AchievementId);
                x.Property(e => e.AchievementId).ValueGeneratedOnAdd();
                x.HasData(new List<Achievement>
                {
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 5",
                        Description = "Reach Server Level 5",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 5,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 10",
                        Description = "Reach Server Level 10",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 10,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 20",
                        Description = "Reach Server Level 20",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 20,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 30",
                        Description = "Reach Server Level 30",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 30,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 40",
                        Description = "Reach Server Level 40",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 40,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 50",
                        Description = "Reach Server Level 50",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 50,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Rare,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 60",
                        Description = "Reach Server Level 60",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 60,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Rare,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Reach Server Level 70",
                        Description = "Reach Server Level 70",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 70,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Epic,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 80",
                        Description = "Reach Server Level 80",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 80,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Epic,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 90",
                        Description = "Reach Server Level 90",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 90,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Legendary,
                        Unlocked = null
                    },
                    new Achievement
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 100",
                        Description = "Reach Server Level 100",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 100,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Legendary,
                        Unlocked = null
                    }
                });
            });
            modelBuilder.Entity<AchievementUnlocked>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasOne(e => e.Achievement).WithMany(e => e.Unlocked);
               x.HasOne(e => e.Account).WithMany(e => e.AchievementUnlocks);
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
            });
            modelBuilder.Entity<MuteTimer>(x =>
            {
                x.HasKey(e => new {e.UserId, e.GuildId});
            });
            modelBuilder.Entity<Suggestion>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
            modelBuilder.Entity<Warn>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
            modelBuilder.Entity<ApprovalQueue>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.Type).HasConversion(
                v => v.ToString(), 
                v => (ApprovalQueueType)Enum.Parse(typeof(ApprovalQueueType), v)); 
            });
        }

        private static void ClubBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Club>(x =>
            {
                x.HasKey(e => new {e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();

                x.HasMany(e => e.Users)
                    .WithOne(e => e.Club)
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);
                x.HasMany(e => e.Blacklist)
                    .WithOne(e => e.Club)
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<ClubUser>(x =>
            {
                x.HasKey(e => new {e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ClubBlacklist>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.User});
            });
        }

        private static void ConfigBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Premium).HasDefaultValue(null);
            });
            modelBuilder.Entity<AdminConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<BoardConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<ChannelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.HasMany(e => e.AssignReactionRoles).WithOne(e => e.Config).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ClubConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<CurrencyConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.EmoteCurrency).HasDefaultValue(false);
                x.Property(e => e.SpecialEmoteCurrency).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.BoostExpMultiplier).HasDefaultValue(1);
                x.Property(e => e.Decay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LoggingConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<SuggestionConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<WelcomeConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<DropConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });

            modelBuilder.Entity<IgnoreChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
            modelBuilder.Entity<Board>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.MessageId});
            });
            modelBuilder.Entity<WelcomeBanner>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.AvatarSize).HasDefaultValue(60);
                x.Property(e => e.AviPlaceX).HasDefaultValue(10);
                x.Property(e => e.AviPlaceY).HasDefaultValue(10);
                x.Property(e => e.TextSize).HasDefaultValue(33);
                x.Property(e => e.TextPlaceX).HasDefaultValue(245);
                x.Property(e => e.TextPlaceY).HasDefaultValue(40);
            });
            modelBuilder.Entity<LootChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
            modelBuilder.Entity<LevelExpReduction>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
            modelBuilder.Entity<SelfAssignAbleRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId});
            });
            modelBuilder.Entity<SelfAssignReactionRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId, e.MessageId});
            });
            modelBuilder.Entity<BoostConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
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
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/epIb29P.png"
                },
                new Background
                {
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/04PbzvT.png"
                },
                new Background
                {
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/5ojmh76.png"
                },
                new Background
                {
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/OAMpNDh.png"
                },
                new Background
                {
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/KXO5bx5.png"
                },
                new Background
                {
                    Id = Guid.NewGuid(),
                    BackgroundUrl = "https://i.imgur.com/5h5zZ7C.png"
                }
            });
        });

        private static void PremiumBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MvpConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Day).HasConversion(
                    v => v.ToString(),
                    v => (DayOfWeek) Enum.Parse(typeof(DayOfWeek), v));
                x.Property(e => e.Disabled).HasDefaultValue(true);
            });
        }

        private static void HungerGameBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HungerGame>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<HungerGameHistory>(x =>
            {
                x.HasKey(e => e.GameId);
            });
            modelBuilder.Entity<HungerGameProfile>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.UserId });
            });
            modelBuilder.Entity<HungerGameStatus>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<HungerGameCustomChar>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<HungerGameDefault>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasData(new List<HungerGameDefault>
                {
                    new HungerGameDefault
                    {
                        Id = 1,
                        Name = "Dia",
                        Avatar = "https://i.imgur.com/XMjW8Qn.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 2,
                        Name = "Kanan",
                        Avatar = "https://i.imgur.com/7URjbvT.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 3,
                        Name = "Yoshiko",
                        Avatar = "https://i.imgur.com/tPDON9P.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 4,
                        Name = "Kongou",
                        Avatar = "https://i.imgur.com/dcB1loo.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 5,
                        Name = "Haruna",
                        Avatar = "https://i.imgur.com/7GC7FvJ.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 6,
                        Name = "Yamato",
                        Avatar = "https://i.imgur.com/8748bUL.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 7,
                        Name = "Akagi",
                        Avatar = "https://i.imgur.com/VLsezdF.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 8,
                        Name = "Kaga",
                        Avatar = "https://i.imgur.com/eyt9k8E.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 9,
                        Name = "Zero Two",
                        Avatar = "https://i.imgur.com/4XYg6ch.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 10,
                        Name = "Echidna",
                        Avatar = "https://i.imgur.com/Nl6WsbP.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 11,
                        Name = "Emilia",
                        Avatar = "https://i.imgur.com/kF9b4SJ.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 12,
                        Name = "Rem",
                        Avatar = "https://i.imgur.com/y3bb8Sk.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 13,
                        Name = "Ram",
                        Avatar = "https://i.imgur.com/5CcdVBE.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 14,
                        Name = "Gura",
                        Avatar = "https://i.imgur.com/0VYBYEg.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 15,
                        Name = "Shiki",
                        Avatar = "https://i.imgur.com/rYa5iYc.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 16,
                        Name = "Chika",
                        Avatar = "https://i.imgur.com/PT8SsVB.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 17,
                        Name = "Sora",
                        Avatar = "https://i.imgur.com/5xR0ImK.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 18,
                        Name = "Nobuna",
                        Avatar = "https://i.imgur.com/U0NlfJd.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 19,
                        Name = "Akame",
                        Avatar = "https://i.imgur.com/CI9Osi5.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 20,
                        Name = "Shiina",
                        Avatar = "https://i.imgur.com/GhSG97V.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 21,
                        Name = "Bocchi",
                        Avatar = "https://i.imgur.com/VyJf95i.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 22,
                        Name = "Enterprise",
                        Avatar = "https://i.imgur.com/bv5ao8Z.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 23,
                        Name = "Chocola",
                        Avatar = "https://i.imgur.com/HoNwKi9.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 24,
                        Name = "Vanilla",
                        Avatar = "https://i.imgur.com/aijxHla.png"
                    },
                    new HungerGameDefault
                    {
                        Id = 25,
                        Name = "Shiro",
                        Avatar = "https://i.imgur.com/Wxhd5WY.png"
                    }
                });
            });
        }

        private static void InternalBuilder(ModelBuilder modelBuilder) => modelBuilder.Entity<Log>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Id).ValueGeneratedOnAdd();
        });
    }
}