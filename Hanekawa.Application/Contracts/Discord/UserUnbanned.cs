using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class UserUnbanned : ISqs
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
}