using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record UserBanned(ulong GuildId, ulong UserId) : ISqs<bool>;