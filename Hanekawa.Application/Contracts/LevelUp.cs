using MediatR;

namespace Hanekawa.Application.Contracts;

public record LevelUp(ulong GuildId, ulong UserId, HashSet<ulong> RoleIds, int Level) : IRequest;