using System.Threading;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Entities.Account;
using Hanekawa.Entities.Account.Profile;
using Hanekawa.Entities.Account.Stores;
using Hanekawa.Entities.Administration;
using Hanekawa.Entities.Advertise;
using Hanekawa.Entities.AutoMessage;
using Hanekawa.Entities.BoardConfig;
using Hanekawa.Entities.Config;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Entities.Config.Level;
using Hanekawa.Entities.Config.SelfAssign;
using Hanekawa.Entities.Game.ShipGame;
using Hanekawa.Entities.Giveaway;
using Hanekawa.Entities.Internal;
using Hanekawa.Entities.Moderation;
using Hanekawa.Entities.Premium;
using Hanekawa.Entities.Quote;
using Hanekawa.Entities.SelfAssign;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Hanekawa.Infrastructure
{
    public partial class DbService : DbContext, IDbContext
    {
        public DbService(DbContextOptions<DbService> options) : base(options) { }

        public DbSet<VoiceRoles> VoiceRoles { get; set; }
        public DbSet<AutoMessage> AutoMessages { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountGlobal> AccountGlobals { get; set; }
        public DbSet<AdminConfig> AdminConfigs { get; set; }
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<MuteTimer> MuteTimers { get; set; }
        public DbSet<Warn> Warns { get; set; }
        public DbSet<TopGG> Top { get; set; }
        public DbSet<VoteLog> VoteLogs { get; set; }
        public DbSet<BoardConfig> BoardConfigs { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoostConfig> BoostConfigs { get; set; }
        public DbSet<ClubConfig> ClubConfigs { get; set; }
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<ChannelConfig> ChannelConfigs { get; set; }
        public DbSet<LoggingConfig> LoggingConfigs { get; set; }
        public DbSet<DropConfig> DropConfigs { get; set; }
        public DbSet<GameClass> GameClasses { get; set; }
        public DbSet<GameConfig> GameConfigs { get; set; }
        public DbSet<GameEnemy> GameEnemies { get; set; }
        public DbSet<Giveaway> Giveaways { get; set; }
        public DbSet<GiveawayParticipant> GiveawayParticipants { get; set; }
        public DbSet<GiveawayHistory> GiveawayHistories { get; set; }
        public DbSet<CurrencyConfig> CurrencyConfigs { get; set; }
        public DbSet<ServerStore> ServerStores { get; set; }
        public DbSet<Background> Backgrounds { get; set; }
        public DbSet<LevelConfig> LevelConfigs { get; set; }
        public DbSet<LevelReward> LevelRewards { get; set; }
        public DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        public DbSet<MvpConfig> MvpConfigs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }
        public DbSet<SelfAssignReactionRole> SelfAssignReactionRoles { get; set; }
        public DbSet<SelfAssignGroup> SelfAssignGroups { get; set; }
        public DbSet<SelfAssignItem> SelfAssignItems { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<SuggestionConfig> SuggestionConfigs { get; set; }
        public DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public DbSet<WelcomeConfig> WelcomeConfigs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AccountBuilder(modelBuilder);
            AdministrationBuilder(modelBuilder);
            AdvertisementBuilder(modelBuilder);
            BoardBuilder(modelBuilder);
            BoostBuilder(modelBuilder);
            ConfigBuilder(modelBuilder);
            DropBuilder(modelBuilder);
            GameBuilder(modelBuilder);
            GiveawayBuilder(modelBuilder);
            // TODO: Recreate hunger game entities
            //HungerGameBuilder(modelBuilder);
            InventoryStoreBuilder(modelBuilder);
            LevelBuilder(modelBuilder);
            MvpBuilder(modelBuilder);
            ReportBuilder(modelBuilder);
            SelfAssignableBuilder(modelBuilder);
            SuggestionBuilder(modelBuilder);
            WelcomeBuilder(modelBuilder);

            modelBuilder.Entity<Log>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Blacklist>(x => { x.HasKey(e => e.GuildId); });
            modelBuilder.Entity<VoiceRoles>(x => { x.HasKey(e => new {e.GuildId, e.VoiceId}); });
            modelBuilder.Entity<AutoMessage>(x => { x.HasKey(e => new {e.GuildId, e.Name}); });
            modelBuilder.Entity<Quote>(x => { x.HasKey(e => new {e.GuildId, e.Key}); });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ctx = default) 
            => await base.SaveChangesAsync(ctx);

        public override async ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
            => await base.FindAsync<TEntity>(keyValues);

        public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity) where TEntity : class
            => await base.AddAsync(entity);
    }
}