using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record ReactionAdd(ulong GuildId, ulong ChannelId, ulong MessageId, ulong UserId, string? Emoji) : ISqs<bool>;