namespace Hanekawa.Application.Interfaces.Services;

/// <summary>
/// Shared rate limiter for message-based features (experience, weekly activity).
/// Backed by a distributed cache so the cooldown is consistent across instances.
/// </summary>
public interface IMessageRateLimiter
{
    /// <summary>
    /// Returns true if the user is allowed through (and starts the cooldown),
    /// false if the user is still on cooldown.
    /// </summary>
    Task<bool> TryPassAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default);
}
