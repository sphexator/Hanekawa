using Hanekawa.Application.Interfaces;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Services.Warnings;

public record WarningClear(DiscordMember User, ulong ModeratorId, string? Reason, bool All = false) 
    : IRequest<Response<Message>>;

public class WarningClearHandler(IDbContext db, ILogger<WarningClearHandler> logger) 
    : IRequestHandler<WarningClear, Response<Message>>
{
    public async Task<Response<Message>> HandleAsync(WarningClear request, CancellationToken cancellationToken)
    {
        if (request.All)
        {
            var warnings = await db.Warnings.Where(x => x.GuildId == request.User.Guild.GuildId
                                                         && x.UserId == request.User.Id
                                                         && x.Valid)
                .ToArrayAsync(cancellationToken: cancellationToken);
            for (var i = 0; i < warnings.Length; i++)
            {
                var x = warnings[i];
                x.Valid = false;
            }
            await db.SaveChangesAsync();
            return new Response<Message>(new Message(string.Format(Localization.ClearedAllWarnUserMention, 
                request.User.Mention)));
        }
        var warning = await db.Warnings.FirstOrDefaultAsync(x => x.GuildId == request.User.Guild.GuildId
                                                                  && x.UserId == request.User.Id
                                                                  && x.Valid,
            cancellationToken: cancellationToken);
        if (warning is null)
        {
            return new Response<Message>(new Message(string.Format(Localization.NoWarningsUserMention, 
                request.User.Mention)));
        }

        warning.Valid = false;
        db.Warnings.Update(warning);
        await db.SaveChangesAsync();
        return new Response<Message>(new Message(string.Format(Localization.ClearedWarningUserMention, 
            request.User.Mention)));
    }
}