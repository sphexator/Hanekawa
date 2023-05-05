using Hanekawa.Entities.Levels;

namespace Hanekawa.Application.Interfaces.Commands;

public interface ILevelCommandService
{
    Task AddAsync(ulong guildId, ulong roleId, int level, CancellationToken cancellationToken = default);
    Task RemoveAsync(ulong guildId, ulong roleId, CancellationToken cancellationToken = default);
    Task<List<LevelReward>> ListAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task ModifyAsync(ulong guildId, ulong roleId, int level, CancellationToken cancellationToken = default);
    
    Task CreateBoostPeriodAsync(ulong guildId, TimeSpan duration, CancellationToken cancellationToken = default);
    Task RemoveBoostPeriodAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListBoostPeriodsAsync(ulong guildId, CancellationToken cancellationToken = default);
}