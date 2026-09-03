using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class StreamUser : IConfig
{
    [Key]
    public int Id { get; init; }

    public ulong GuildId { get; init; }

    public ulong DiscordUserId { get; init; }

    public string TwitchLogin { get; set; } = string.Empty;

    public string? TwitchUserId { get; set; }

    [JsonIgnore]
    public StreamConfig? StreamConfig { get; set; }
}
