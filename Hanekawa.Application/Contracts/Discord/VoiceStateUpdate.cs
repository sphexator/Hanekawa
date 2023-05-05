using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class VoiceStateUpdate : ISqs
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong? ChannelId { get; set; }
    public string? SessionId { get; set; }
}