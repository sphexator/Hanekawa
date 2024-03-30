using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Commands.Account;

public class AccountCommandService(IImageService imageService, IDbContext db)
{
    public async ValueTask<Stream> ProfileAsync(DiscordMember member, CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(x => x.User)
            .GetOrCreateAsync(x => x.GuildId == member.Guild.Id && 
                                   x.UserId == member.Id,
                new(member.Guild.Id, member.Id),
                cancellationToken);
        return await imageService.DrawProfileAsync(member, user, cancellationToken);
    }
}