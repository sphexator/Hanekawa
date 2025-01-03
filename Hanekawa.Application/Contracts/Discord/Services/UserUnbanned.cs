using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record UserUnbanned(DiscordMember Member) : INotificationSqs
{
    public ulong GuildId { get; init; }
}