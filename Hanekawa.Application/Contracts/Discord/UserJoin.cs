using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record UserJoin(ulong GuildId, ulong UserId, string Username, 
    string AvatarUrl, DateTimeOffset CreatedAt) : ISqs<bool>;