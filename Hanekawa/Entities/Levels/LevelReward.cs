using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Entities.Configs;

namespace Hanekawa.Entities.Levels;

public class LevelReward
{
    [Key]
    public int Level { get; set; }

    public ulong? RoleId { get; set; } = null;
    public int? Money { get; set; } = null;
    
    public ulong GuildId { get; set; }
    
    [JsonIgnore]
    public LevelConfig LevelConfig { get; set; }
}