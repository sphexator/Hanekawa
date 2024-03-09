using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record ReactionRemove(ulong GuildId, ulong ChannelId, ulong MessageId, ulong UserId, string? Emoji) : ISqs<bool>;