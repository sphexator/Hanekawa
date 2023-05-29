using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record ReactionCleared(ulong GuildId, ulong ChannelId, ulong MessageId) : ISqs<bool>;