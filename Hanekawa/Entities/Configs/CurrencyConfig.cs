using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class CurrencyConfig : IConfig
{
    [Key]
    public ulong GuildId { get; init; }
    public string CurrencyName { get; set; } = "Hanekawa Coins";
    public string CurrencySymbol { get; set; } = "$";
    public AffixType SymbolAffix { get; set; } = AffixType.Prefix;
    public bool IsEmote { get; set; } = false;

    [JsonIgnore]
    public GuildConfig? GuildConfig { get; set; }
}