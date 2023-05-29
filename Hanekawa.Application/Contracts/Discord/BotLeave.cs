using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record BotLeave(ulong GuildId) : ISqs<bool>;