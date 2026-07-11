using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Pipelines;

public sealed class WarningAdded(
    IRequestHandler<WarningReceived, Response<Message>> inner,
    IDbContext db,
    IBot bot) : IPipelineHandler<WarningReceived, Response<Message>>
{
    public async Task<Response<Message>> HandleAsync(WarningReceived request,
        CancellationToken cancellationToken)
    {
        var result = await inner.HandleAsync(request, cancellationToken).ConfigureAwait(false);
        var config = await db.GuildConfigs.Include(x => x.AdminConfig)
            .FirstOrDefaultAsync(x => x.GuildId == request.User.Guild.GuildId,
                cancellationToken: cancellationToken);
        var warningCount = await db.Warnings.CountAsync(x => x.GuildId == request.User.Guild.GuildId
                                                             && x.UserId == request.User.Id
                                                             && x.Valid
                                                             && x.CreatedAt > DateTimeOffset.UtcNow.AddDays(-7), cancellationToken);

        // TODO: Add a warning threshold to the guild configuration
        if (warningCount >= config?.AdminConfig?.MaxWarnings)
            await bot.MuteAsync(request.User.Guild.GuildId, request.User.Id,
                $"Auto-mod warning threshold reached ({config.AdminConfig.MaxWarnings})",
                TimeSpan.FromHours(2 * Convert.ToDouble(warningCount / 3)));
        return result;
    }
}