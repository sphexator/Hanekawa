using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class AdminConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public int MaxWarnings { get; set; }
    
    
    public GuildConfig GuildConfig { get; set; }
}