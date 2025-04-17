using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GuildConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public string Prefix { get; set; } = "h.";
    public string Language { get; set; } = "en-US";

    public GreetConfig? GreetConfig { get; set; } = new();
    public LevelConfig? LevelConfig { get; set; } = new();
    public LogConfig? LogConfig { get; set; } = new();
    public AdminConfig? AdminConfig { get; set; } = new();
    public DropConfig? DropConfig { get; set; } = new();
    public CurrencyConfig? CurrencyConfig { get; set; } = new();
}