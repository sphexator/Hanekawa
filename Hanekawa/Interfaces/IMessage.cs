using System;

namespace Hanekawa.Interfaces
{
    public interface IMessage
    {
        ulong Id { get; set; }
        ulong ChannelId { get; set; }
        ulong GuildId { get; set; }
        ulong UserId { get; set; }
        string Content { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset? EditedAt { get; set; }
        bool IsPinned { get; set; }
    }
}