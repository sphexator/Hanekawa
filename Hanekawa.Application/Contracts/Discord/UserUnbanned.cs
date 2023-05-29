using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record UserUnbanned(ulong GuildId, ulong UserId) : ISqs<bool>;