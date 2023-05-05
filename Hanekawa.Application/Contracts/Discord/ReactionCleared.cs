using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class ReactionCleared : ISqs
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
}