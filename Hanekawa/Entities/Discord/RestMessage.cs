using System;

namespace Hanekawa.Entities.Discord;

public class RestMessage
{
    public ulong Id { get; set; }
    public ulong ChannelId { get; set; }
    
    public string? Content { get; set; } = null;
    public Embed? Embed { get; set; } = null;
    public string? Attachment { get; set; } = null;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; } = null;
    
    public ulong AuthorId { get; set; }
    public bool IsBot { get; set; } = false;
    
    public ulong? GuildId { get; set; } = null;
    public ulong? CategoryId { get; set; } = null;
}