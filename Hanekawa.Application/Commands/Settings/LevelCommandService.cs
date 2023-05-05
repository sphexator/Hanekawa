using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities.Levels;

namespace Hanekawa.Application.Commands.Settings;

public class LevelCommandService : ILevelCommandService
{
    public Task AddAsync(ulong guildId, ulong roleId, int level, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(ulong guildId, ulong roleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<LevelReward>> ListAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ModifyAsync(ulong guildId, ulong roleId, int level, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateBoostPeriodAsync(ulong guildId, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveBoostPeriodAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> ListBoostPeriodsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}