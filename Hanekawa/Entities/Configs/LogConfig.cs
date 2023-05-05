using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Configs;

public class LogConfig : IConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public ulong? JoinLeaveLogChannelId { get; set; }
    public ulong? MessageLogChannelId { get; set; }
    public ulong? ModLogChannelId { get; set; }
    public ulong? VoiceLogChannelId { get; set; }
    
    public GuildConfig GuildConfig { get; set; } = null!;
}