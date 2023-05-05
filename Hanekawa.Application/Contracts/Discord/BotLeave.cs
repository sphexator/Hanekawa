using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public class BotLeave : ISqs
{
    public ulong GuildId { get; set; }
}