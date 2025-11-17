using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Discord;

public class SimpleGuild : IGuildEntity
{
    public ulong GuildId { get; init; }
    public string Name { get; init; } = null!;
    public string IconUrl { get; init; } = null!;
}