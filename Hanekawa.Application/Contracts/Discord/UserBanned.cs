using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class UserBanned : ISqs
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
}