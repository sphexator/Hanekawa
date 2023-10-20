namespace Hanekawa.Entities.Discord;

public class TextChannel
{
    public TextChannel() => Mention = $"<#{Id}>";

    public ulong Id { get; set; }
    public ulong? Category { get; set; } = null;
    public string Name { get; set; } = null!;
    public string Mention { get; set; }
    public ulong? GuildId { get; set; } = null;
    public bool IsNsfw { get; set; }
}