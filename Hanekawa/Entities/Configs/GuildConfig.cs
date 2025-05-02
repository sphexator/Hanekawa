using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
    public BoostConfig? BoostConfig { get; set; } = new();
}

public class BoostConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public int Experience { get; set; }
    public int Currency { get; set; }
    public decimal ExperienceMultiplier { get; set; }
    public decimal CurrencyMultiplier { get; set; }

    public bool ReoccurringRewards { get; set; }

    public bool Enabled { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public GuildConfig? GuildConfig { get; set; }
}