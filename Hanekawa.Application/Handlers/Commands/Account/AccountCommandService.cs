using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;

namespace Hanekawa.Application.Handlers.Commands.Account;

public class AccountCommandService(IImageService imageService, IDbContext db)
{
    public async ValueTask<Stream> ProfileAsync(DiscordMember member, CancellationToken cancellationToken = default)
    {
        var user = await db.GetOrCreateUserAsync(member.Guild.GuildId, member.Id, cancellationToken).ConfigureAwait(false);
        return await imageService.DrawProfileAsync(member, user, cancellationToken);
    }
}