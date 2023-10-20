using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Application.Interfaces.Services;

public interface ILevelService
{
    Task<int?> AddExperienceAsync(DiscordMember member, int experience); 
    Task<DiscordMember> AdjustRolesAsync(DiscordMember member, int level, GuildConfig config);
}