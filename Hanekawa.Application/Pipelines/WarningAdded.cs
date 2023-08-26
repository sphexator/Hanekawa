﻿using Hanekawa.Application.Handlers.Warnings;
using Hanekawa.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Pipelines;

public class WarningAdded : IPipelineBehavior<WarningReceived, WarningReceivedHandler>
{
    private readonly IDbContext _db;
    private readonly IBot _bot;

    public WarningAdded(IDbContext db, IBot bot)
    {
        _db = db;
        _bot = bot;
    }

    public async Task<WarningReceivedHandler> Handle(WarningReceived request, 
        RequestHandlerDelegate<WarningReceivedHandler> next, 
        CancellationToken cancellationToken)
    {
        var result = await next();
        var config = await _db.GuildConfigs.Include(x => x.AdminConfig)
            .FirstOrDefaultAsync(x => x.GuildId == request.User.Guild.Id, 
                cancellationToken: cancellationToken);
        var warningCount =
            await _db.Warnings.CountAsync(x => x.GuildId == request.User.Guild.Id 
                                               && x.UserId == request.User.Id
                                               && x.Valid
                                               && x.CreatedAt > DateTimeOffset.UtcNow.AddDays(-7), 
                cancellationToken);
        // TODO: Add a warning threshold to the guild configuration
        if (warningCount >= config?.AdminConfig.MaxWarnings)
            await _bot.MuteAsync(request.User.Guild.Id, request.User.Id, 
                $"Auto-mod warning threshold reached ({config.AdminConfig.MaxWarnings})",
                TimeSpan.FromHours(2 * Convert.ToDouble(warningCount / 3)));
        return result;
    }
}