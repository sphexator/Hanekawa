using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GuildConfig : IConfig
{
    [Key]
    public ulong GuildId { get; init; }
    public string Prefix { get; set; } = "h.";
    public string Language { get; set; } = "en-US";

    public DateTimeOffset? MarkedForDeletion { get; set; } = null;

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public GreetConfig? GreetConfig { get; set; } = new();
    public LevelConfig? LevelConfig { get; set; } = new();
    public LogConfig? LogConfig { get; set; } = new();
    public AdminConfig? AdminConfig { get; set; } = new();
    public DropConfig? DropConfig { get; set; } = new();
    public CurrencyConfig? CurrencyConfig { get; set; } = new();
    public BoostConfig? BoostConfig { get; set; } = new();
    public StreamConfig? StreamConfig { get; set; } = new();
}