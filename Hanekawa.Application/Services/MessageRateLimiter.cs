using Hanekawa.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class MessageRateLimiter(IDistributedCache cache, ILogger<MessageRateLimiter> logger) : IMessageRateLimiter
{
    internal static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);

    private static readonly DistributedCacheEntryOptions Options = new()
    {
        AbsoluteExpirationRelativeToNow = Cooldown
    };

    /// <inheritdoc />
    public async Task<bool> TryPassAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        var key = Key(guildId, userId);
        var existing = await cache.GetStringAsync(key, cancellationToken);
        if (existing is not null)
        {
            logger.LogDebug("User {User} in guild {Guild} is on message cooldown", userId, guildId);
            return false;
        }

        await cache.SetStringAsync(key, "1", Options, cancellationToken);
        return true;
    }

    internal static string Key(ulong guildId, ulong userId) => $"rate-limit:message:{guildId}:{userId}";
}
