using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record MessageDeleted
    (ulong GuildId, ulong ChannelId, ulong AuthorId, ulong MessageId, string MessageContent) : ISqs<bool>;
