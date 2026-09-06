using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record ReactionCleared(ulong GuildId, ulong ChannelId, ulong MessageId) : INotificationSqs;

public record MemberUpdated(ulong GuildId, DiscordMember oldMember, DiscordMember newMember) : INotificationSqs;

public record PresenceUpdated(ulong GuildId, ulong UserId, string[] Activities) : INotificationSqs;