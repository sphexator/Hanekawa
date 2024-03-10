using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using MediatR;

namespace Hanekawa.Application.Handlers.Services.Warnings;

public record WarningReceived(DiscordMember User, string Warning, ulong ModeratorId) : IRequest<Response<Message>>;

public class WarningReceivedHandler(IDbContext db) : IRequestHandler<WarningReceived, Response<Message>>
{
    public async Task<Response<Message>> Handle(WarningReceived request, CancellationToken cancellationToken)
    {
        await db.Warnings.AddAsync(new()
        {
            Id = Guid.NewGuid(),
            GuildId = request.User.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.ModeratorId,
            Reason = request.Warning,
            Valid = true,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await db.SaveChangesAsync();
        // Change to mention the user
        return new(new(string.Format(Localization.WarnedUser, request.User.Mention)));
    }
}