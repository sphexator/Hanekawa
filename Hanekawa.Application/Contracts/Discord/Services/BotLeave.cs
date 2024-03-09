using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record BotLeave(ulong GuildId) : ISqs<bool>;