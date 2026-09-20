using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord.Services;

/// <summary>
/// Published when a message passes the shared message rate limiter.
/// Consumed by features that should only count non-spam messages (experience, weekly activity).
/// </summary>
public record MessageRateLimitPassed(ulong GuildId, ulong ChannelId, DiscordMember Member,
    ulong MessageId, string? Message, DateTimeOffset CreatedAt) : INotificationSqs;
