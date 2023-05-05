using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts;

public class LevelUp : ISqs
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public HashSet<ulong> RoleIds { get; set; } = null!;
    public int Level { get; set; }
}