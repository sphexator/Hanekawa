using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record VoiceStateUpdate(ulong GuildId, ulong UserId, ulong? ChannelId, string? SessionId) : ISqs<bool>;