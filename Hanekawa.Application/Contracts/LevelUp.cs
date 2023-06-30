using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Contracts;

public record LevelUp(DiscordMember Member, HashSet<ulong> RoleIds, int Level, GuildConfig GuildConfig) : IRequest;