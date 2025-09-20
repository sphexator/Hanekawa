using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class BoostConfig : IConfig
{
    [Key]
    public ulong GuildId { get; init; }
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