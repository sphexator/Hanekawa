using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class GreetImage : IConfig
{
    [Key]
    public int Id { get; init; } = 0;
    public ulong GuildId { get; init; } = 0;
    public string ImageUrl { get; init; } = null!;
    public ulong Uploader { get; init; } = 0;

    public int AvatarSize { get; set; } = 128;
    public int AvatarX { get; set; } = 0;
    public int AvatarY { get; set; } = 0;

    public float UsernameSize { get; set; } = 32;
    public int UsernameX { get; set; } = 0;
    public int UsernameY { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public GreetConfig? GreetConfig { get; init; } = null!;
}