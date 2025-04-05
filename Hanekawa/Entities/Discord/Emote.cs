namespace Hanekawa.Entities.Discord;

public class Emote
{
    public required ulong Id { get; set; }
    public required string Name { get; set; } = null!;
    public required string Format { get; set; } = null!;
    public required bool IsAvailable { get; set; } = false;
    public required bool IsAnimated { get; set; } = false;
    public required bool IsManaged { get; set; } = true;
}