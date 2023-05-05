﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GreetConfig : IConfig
{
    public GreetConfig() { }
    
    public GreetConfig(ulong guildId) => GuildId = guildId;

    [Key]
    public ulong GuildId { get; set; }
    public string Message { get; set; } = "";
    public ulong? Channel { get; set; }
    
    public bool ImageEnabled { get; set; } = false;
    public string ImageUrl { get; set; } = "";

    public bool DmEnabled { get; set; } = false;
    public string DmMessage { get; set; } = "";
    
    public GuildConfig GuildConfig { get; set; }
    public List<GreetImage> Images { get; set; }
}