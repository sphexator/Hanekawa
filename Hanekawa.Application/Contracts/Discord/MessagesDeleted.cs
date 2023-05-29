using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord;

public record MessagesDeleted(ulong GuildId, ulong ChannelId, ulong[] AuthorId, 
    ulong[] MessageIds, string[] MessageContents) : ISqs<bool>;