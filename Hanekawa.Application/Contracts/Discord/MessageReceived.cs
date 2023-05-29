using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord;

public record MessageReceived(ulong GuildId, ulong ChannelId, DiscordMember Member, 
    ulong MessageId, string? Message, DateTimeOffset CreatedAt) : ISqs<bool>;