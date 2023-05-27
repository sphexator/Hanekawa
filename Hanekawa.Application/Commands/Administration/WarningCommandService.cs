using Hanekawa.Application.Handlers.Warnings;
using Hanekawa.Application.Interfaces.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Application.Commands.Administration;

public class WarningCommandService : IWarningCommandService
{
    private readonly IServiceProvider _serviceProvider;

    public WarningCommandService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public async Task WarnUserAsync(ulong guildId, ulong userId, ulong moderatorId, string reason) 
        => await _serviceProvider.GetRequiredService<IMediator>()
            .Send(new WarningReceived(guildId, userId, reason, moderatorId));
    /// <inheritdoc />
    public async Task<List<string>> Warns(ulong guildId) 
        => await _serviceProvider.GetRequiredService<IMediator>()
            .Send(new WarningList(guildId, null));
    /// <inheritdoc />
    public async Task<List<string>> WarnsAsync(ulong guildId, ulong userId) 
        => await _serviceProvider.GetRequiredService<IMediator>()
            .Send(new WarningList(guildId, userId));
    /// <inheritdoc />
    public async Task ClearUserWarnAsync(ulong guildId, ulong userId, ulong moderatorId, string reason, bool all = false) 
        => await _serviceProvider.GetRequiredService<IMediator>()
            .Send(new WarningClear(guildId, userId, moderatorId, reason, all));
}