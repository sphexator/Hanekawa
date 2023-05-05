using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces.Services;

public interface ILevelService
{
    Task<int?> AddExperience(DiscordMember member, int experience); 
    Task<DiscordMember> AdjustRoles(DiscordMember member, int level, GuildConfig config);
}