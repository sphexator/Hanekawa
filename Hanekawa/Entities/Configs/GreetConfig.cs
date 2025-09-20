using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GreetConfig : IConfig
{
    public GreetConfig() { }

    public GreetConfig(ulong guildId) => GuildId = guildId;

    [Key]
    public ulong GuildId { get; init; }
    public string Message { get; set; } = string.Empty;
    public ulong? Channel { get; set; }

    public bool ImageEnabled { get; set; } = false;

    public bool DmEnabled { get; set; } = false;
    public string DmMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public GuildConfig? GuildConfig { get; set; }
    public List<GreetImage> Images { get; set; } = [];
}