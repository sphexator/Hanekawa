using Hanekawa.Decorator;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Contracts;

public record LevelUp(DiscordMember Member, ulong[] RoleIds, int Level, GuildConfig GuildConfig) : IRequest;