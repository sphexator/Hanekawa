using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Discord;

public class DiscordMember : IMemberEntity
{
    public ulong Id { get; set; }
    public ulong GuildId { get; init; }
    public Guild Guild { get; set; } = null!;
    public ulong[] RoleIds { get; set; } = [];
    public string? Nickname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Mention => $"<@{Username}>";
    public string DisplayName => Nickname ?? Username;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsBot { get; set; }

    public string? VoiceSessionId { get; set; } = string.Empty;
}