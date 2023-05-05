using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class UserJoin : ISqs
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string AvatarUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}