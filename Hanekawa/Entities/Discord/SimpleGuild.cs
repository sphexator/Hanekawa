namespace Hanekawa.Entities.Discord;

public class SimpleGuild
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
}