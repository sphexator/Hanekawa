namespace Hanekawa.Entities.Discord;

public class TextChannel
{
    public TextChannel() => Mention = $"<#{Id}>";

    public ulong Id { get; init; }
    public ulong? Category { get; init; } = null;
    public string Name { get; init; } = null!;
    public string Mention { get; init; }
    public ulong? GuildId { get; init; } = null;
    public bool IsNsfw { get; init; }
}