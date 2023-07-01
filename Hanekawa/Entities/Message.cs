using Hanekawa.Entities.Discord;

namespace Hanekawa.Entities;

public class Message
{
    public Message(string content, bool allowMentions = false, bool ephemeral = true)
    {
        Content = content;
        AllowMentions = allowMentions;
        Emphemeral = ephemeral;
    }
    
    public Message(Embed embed, bool allowMentions = false, bool ephemeral = true)
    {
        Embed = embed;
        AllowMentions = allowMentions;
        Emphemeral = ephemeral;
    }
    
    public string Content { get; set; } = null!;
    public Embed Embed { get; set; } = null!;
    public bool AllowMentions { get; set; }
    public bool Emphemeral { get; set; }
}