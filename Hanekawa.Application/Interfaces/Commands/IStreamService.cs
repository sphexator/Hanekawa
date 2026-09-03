using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using OneOf;
using OneOf.Types;

namespace Hanekawa.Application.Interfaces.Commands;

public interface IStreamService
{
    Task<string> SetChannel(ulong guildId, TextChannel channel);
    Task<string> TogglePublish(ulong guildId);
    Task<string> AddUser(ulong guildId, ulong discordUserId, string twitchLogin);
    Task<bool> RemoveUser(ulong guildId, ulong discordUserId);
    Task<OneOf<NotFound, List<StreamUser>>> ListUsers(ulong guildId);
}
