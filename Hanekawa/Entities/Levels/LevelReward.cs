using System.ComponentModel.DataAnnotations;
using Hanekawa.Entities.Configs;

namespace Hanekawa.Entities.Levels;

public class LevelReward
{
    [Key]
    public int Level { get; set; }

    public ulong? RoleId { get; set; } = null;
    public int? Money { get; set; } = null;
    
    public ulong GuildId { get; set; }
    public LevelConfig LevelConfig { get; set; }
}