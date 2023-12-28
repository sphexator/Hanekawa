using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class DropConfig : IConfig
{
    /// <inheritdoc />
    [Key]
    public ulong GuildId { get; set; }
    /// <summary>
    /// Emote used in string format. Either UTF or discord format
    /// </summary>
    public string Emote { get; set; } = string.Empty;
    /// <summary>
    /// Experience reward to user claiming. Default = 100
    /// </summary>
    public int ExpReward { get; set; } = 100;
    /// <summary>
    /// List of blacklisted channels or categories
    /// </summary>
    public ulong[] Blacklist { get; set; } = Array.Empty<ulong>();
    
    [JsonIgnore]
    public GuildConfig GuildConfig { get; set; } = null!;
}