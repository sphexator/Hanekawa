using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class AdminConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public int MaxWarnings { get; set; }
    
    [JsonIgnore]
    public GuildConfig GuildConfig { get; set; }
}