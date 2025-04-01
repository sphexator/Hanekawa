using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Commands.Account;

public class AccountCommandService(IImageService imageService, IDbContext db)
{
    public async ValueTask<Stream> RankAsync(DiscordMember member, CancellationToken cancellationToken = default)
    {
        var user = await db.GetOrCreateUserAsync(member.Guild.GuildId, member.Id, cancellationToken).ConfigureAwait(false);
        return await imageService.DrawRankAsync(member, user, cancellationToken);
    }

    public async ValueTask<Stream> ProfileAsync(DiscordMember member, CancellationToken cancellationToken = default)
    {
        var user = await db.GetOrCreateUserAsync(member.Guild.GuildId, member.Id, cancellationToken).ConfigureAwait(false);
        return await imageService.DrawProfileAsync(member, user, cancellationToken);
    }

    public async Task GetWalletAsync(DiscordMember discordMember, CancellationToken cancellationToken = default)
    {
        var user = await db.GetOrCreateUserAsync(discordMember.Guild.GuildId, discordMember.Id, cancellationToken).ConfigureAwait(false);
        var dbConnection = db.GetConnection();
        await dbConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var userCount = await db.ExecuteQuery<int>("COUNT * FROM Users WHERE GuildId = @Id", new { GuildId = user.GuildId }, cancellationToken);
    }

    public Task<GuildUser[]> GetTopUsersAsync(ulong guildId)
    {
        return db.Users
            .Where(u => u.GuildId == guildId && !u.Inactive)
            .OrderByDescending(u => u.Experience)
            .Take(10)
            .ToArrayAsync();
    }
}