namespace Hanekawa.Entities.Discord;

public class Guild : SimpleGuild
{
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public int EmoteCount { get; set; }
    public int? BoostCount { get; set; }
    public int BoostTier { get; set; }
}