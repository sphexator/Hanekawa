using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class MessagesDeleted : ISqs
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong[] AuthorId { get; set; }
    public ulong[] MessageIds { get; set; }
    public string[] MessageContents { get; set; }
}