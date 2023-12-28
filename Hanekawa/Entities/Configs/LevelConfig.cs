using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Entities.Levels;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class LevelConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public bool LevelEnabled { get; set; } = false;
    public bool DecayEnabled { get; set; } = false;

    public int Multiplier { get; set; } = 1;
    public DateTimeOffset MultiplierEnd { get; set; } = DateTimeOffset.MinValue;
    

    [JsonIgnore]
    public GuildConfig GuildConfig { get; set; }
    public List<LevelReward> Rewards { get; set; }
}