using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Discord;

public class SimpleGuild : IGuildEntity
{
    public ulong GuildId { get; set; }
    public string Name { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
}