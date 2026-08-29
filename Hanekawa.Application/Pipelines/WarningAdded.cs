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

        // MaxWarnings defaults to 0, and GuildConfig initializes AdminConfig to new(),
        // so EF Include will not replace it with null when no row exists. Treat 0 as unset.
        var maxWarnings = config?.AdminConfig?.MaxWarnings ?? 0;
        if (maxWarnings > 0 && warningCount >= maxWarnings)
            await bot.MuteAsync(request.User.Guild.GuildId, request.User.Id,
                $"Auto-mod warning threshold reached ({maxWarnings})",
                TimeSpan.FromHours(2 * Convert.ToDouble(warningCount / 3)));
        return result;
    }
}