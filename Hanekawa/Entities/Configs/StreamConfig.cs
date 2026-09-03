using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class StreamConfig : IConfig
{
    public StreamConfig() { }

    public StreamConfig(ulong guildId) => GuildId = guildId;

    [Key]
    public ulong GuildId { get; init; }

    public ulong? Channel { get; set; }

    public bool PublishOnStart { get; set; }

    [JsonIgnore]
    public GuildConfig? GuildConfig { get; set; }

    public List<StreamUser> Users { get; set; } = [];
}
