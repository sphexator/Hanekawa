using Hanekawa.Application.Interfaces;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Hanekawa.Localize;

namespace Hanekawa.Application.Handlers.Services.Warnings;

public record WarningReceived(DiscordMember User, string Warning, ulong ModeratorId) : IRequest<Response<Message>>;

public class WarningReceivedHandler(IDbContext db) : IRequestHandler<WarningReceived, Response<Message>>
{
    public async Task<Response<Message>> HandleAsync(WarningReceived request, CancellationToken cancellationToken)
    {
        await db.Warnings.AddAsync(new Warning
        {
            Id = Guid.NewGuid(),
            GuildId = request.User.Guild.GuildId,
            UserId = request.User.Id,
            ModeratorId = request.ModeratorId,
            Reason = request.Warning,
            Valid = true,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        return new Response<Message>(new Message(string.Format(Localization.WarnedUser, request.User.Mention)));
    }
}