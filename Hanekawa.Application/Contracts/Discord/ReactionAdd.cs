using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class ReactionAdd : ISqs
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
    public ulong UserId { get; set; }
    public string? Emoji { get; set; }
}