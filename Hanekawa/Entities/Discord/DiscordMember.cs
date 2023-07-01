using System.Collections.Generic;

namespace Hanekawa.Entities.Discord;

public class DiscordMember
{
    public DiscordMember() => Mention = $"<@{Username}>";

    public ulong UserId { get; set; }
    public Guild Guild { get; set; }
    public HashSet<ulong> RoleIds { get; set; }
    public string? Nickname { get; set; }
    public string Username { get; set; }
    public string Mention { get; private set; }
    public string AvatarUrl { get; set; }
    public bool IsBot { get; set; }
    
    public string? VoiceSessionId { get; set; }
}