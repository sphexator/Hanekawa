using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class MessageDeleted : ISqs
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong AuthorId { get; set; }
    public ulong MessageId { get; set; }
    public string MessageContent { get; set; }
}