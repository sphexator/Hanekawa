using System.Threading;
using System.Threading.Tasks;
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

namespace Hanekawa.Application.Interfaces
{
    public interface IDbContext
    {
        DbSet<VoiceRoles> VoiceRoles { get; set; }
        DbSet<AutoMessage> AutoMessages { get; set; }
        DbSet<Quote> Quotes { get; set; }
        DbSet<Log> Logs { get; set; }
        
        DbSet<Account> Accounts { get; set; }
        DbSet<AccountGlobal> AccountGlobals { get; set; }

        DbSet<AdminConfig> AdminConfigs { get; set; }
        DbSet<Blacklist> Blacklists { get; set; }

        DbSet<ModLog> ModLogs { get; set; }
        DbSet<MuteTimer> MuteTimers { get; set; }
        DbSet<Warn> Warns { get; set; }

        DbSet<TopGG> Top { get; set; }
        DbSet<VoteLog> VoteLogs { get; set; }
        
        DbSet<BoardConfig> BoardConfigs { get; set; }
        DbSet<Board> Boards { get; set; }
        
        DbSet<BoostConfig> BoostConfigs { get; set; }

        DbSet<GuildConfig> GuildConfigs { get; set; }
        DbSet<ChannelConfig> ChannelConfigs { get; set; }
        DbSet<LoggingConfig> LoggingConfigs { get; set; }
        
        DbSet<DropConfig> DropConfigs { get; set; }
        
        DbSet<GameClass> GameClasses { get; set; }
        DbSet<GameConfig> GameConfigs { get; set; }
        DbSet<GameEnemy> GameEnemies { get; set; }
        
        DbSet<Giveaway> Giveaways { get; set; }
        DbSet<GiveawayParticipant> GiveawayParticipants { get; set; }
        DbSet<GiveawayHistory> GiveawayHistories { get; set; }
        
        DbSet<CurrencyConfig> CurrencyConfigs { get; set; }
        DbSet<ServerStore> ServerStores { get; set; }
        DbSet<Background> Backgrounds { get; set; }
        
        DbSet<LevelConfig> LevelConfigs { get; set; }
        DbSet<LevelReward> LevelRewards { get; set; }
        DbSet<LevelExpEvent> LevelExpEvents { get; set; }
        
        DbSet<MvpConfig> MvpConfigs { get; set; }
        
        DbSet<Report> Reports { get; set; }
        
        DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }
        DbSet<SelfAssignReactionRole> SelfAssignReactionRoles { get; set; }
        DbSet<SelfAssignGroup> SelfAssignGroups { get; set; }
        DbSet<SelfAssignItem> SelfAssignItems { get; set; }
        
        DbSet<Suggestion> Suggestions { get; set; }
        DbSet<SuggestionConfig> SuggestionConfigs { get; set; }
        
        DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        DbSet<WelcomeConfig> WelcomeConfigs { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken ctx = default);
        ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
        Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity) where TEntity : class;
    }
}