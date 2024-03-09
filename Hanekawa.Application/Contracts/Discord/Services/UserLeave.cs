using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record UserLeave(ulong GuildId, ulong UserId) : ISqs<bool>;