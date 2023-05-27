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
    
    public int AvatarSize { get; set; } = 128;
    public int AvatarX { get; set; } = 0;
    public int AvatarY { get; set; } = 0;
    
    public float UsernameSize { get; set; } = 32;
    public int UsernameX { get; set; } = 0;
    public int UsernameY { get; set; } = 0;
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public GreetConfig GreetConfig { get; set; }
}