using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Warnings;

public record WarningClear(DiscordMember user, ulong ModeratorId, string? Reason, bool All = false) : IRequest<Response<Message>>;

public class WarningClearHandler : IRequestHandler<WarningClear, Response<Message>>
{
    private readonly IDbContext _db;
    private readonly ILogger<WarningClearHandler> _logger;

    public WarningClearHandler(IDbContext db, ILogger<WarningClearHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Response<Message>> Handle(WarningClear request, CancellationToken cancellationToken)
    {
        if (request.All)
        {
            var warnings = await _db.Warnings.Where(x => x.GuildId == request.user.Guild.Id
                                                         && x.UserId == request.user.Id
                                                         && x.Valid)
                .ToArrayAsync(cancellationToken: cancellationToken);
            for (var i = 0; i < warnings.Length; i++)
            {
                var x = warnings[i];
                x.Valid = false;
            }
            await _db.SaveChangesAsync();
            return new Response<Message>(new(string.Format(Localization.ClearedAllWarnUserMention, 
                request.user.Mention)));
        }
        var warning = await _db.Warnings.FirstOrDefaultAsync(x => x.GuildId == request.user.Guild.Id
                                                                  && x.UserId == request.user.Id
                                                                  && x.Valid,
            cancellationToken: cancellationToken);
        if (warning is null)
        {
            return new Response<Message>(new(string.Format(Localization.NoWarningsUserMention, 
                request.user.Mention)));
        }

        warning.Valid = false;
        _db.Warnings.Update(warning);
        await _db.SaveChangesAsync();
        return new Response<Message>(new(string.Format(Localization.ClearedWarningUserMention, 
            request.user.Mention)));
    }
}