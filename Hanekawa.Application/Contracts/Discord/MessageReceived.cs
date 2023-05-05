using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord;

public class MessageReceived : ISqs
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public DiscordMember Member { get; set; } = null!;
    public ulong MessageId { get; set; }
    public string? Message { get; set; } = null;
    public DateTimeOffset CreatedAt { get; set; }
}