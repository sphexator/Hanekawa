using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GuildConfig : IConfig
{
    public ulong GuildId { get; set; }
    public string Prefix { get; set; } = "h.";
    public string Language { get; set; } = "en-US";
    
    public GreetConfig? GreetConfig { get; set; }
    public LevelConfig? LevelConfig { get; set; }
    public LogConfig? LogConfig { get; set; }
    public AdminConfig? AdminConfig { get; set; }
    public DropConfig? DropConfig { get; set; }
}