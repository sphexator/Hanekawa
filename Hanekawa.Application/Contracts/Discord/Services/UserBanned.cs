using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record UserBanned(DiscordMember Member) : INotificationSqs
{
    public ulong GuildId { get; init; }
}