using System;
using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GreetImage : IConfig
{
    [Key]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public ulong Uploader { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public GreetConfig GreetConfig { get; set; }
}