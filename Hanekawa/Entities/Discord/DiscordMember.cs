using System.Collections.Generic;

namespace Hanekawa.Entities.Discord;

public class DiscordMember
{
    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public HashSet<ulong> RoleIds { get; set; }
    public string? Nickname { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsBot { get; set; }
    
    public string? VoiceSessionId { get; set; }
}