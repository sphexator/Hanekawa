namespace Hanekawa.Entities.Discord;

public class Guild
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public int EmoteCount { get; set; }
    public int BoostCount { get; set; }
    public int BoostTier { get; set; }
}