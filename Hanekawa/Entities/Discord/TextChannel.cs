namespace Hanekawa.Entities.Discord;

public class TextChannel
{
    public TextChannel() => Mention = $"<#{Id}>";

    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string Mention { get; set; }
    public ulong GuildId { get; set; }
    public bool IsNsfw { get; set; }
}