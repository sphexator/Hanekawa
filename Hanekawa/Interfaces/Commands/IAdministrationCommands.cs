using System;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IAdministrationCommands
    {
        ValueTask BanAsync(ulong guildId, ulong userId, string reason);
        ValueTask UnbanAsync(ulong guildId, ulong userId, string reason);
        ValueTask MassBanAsync(ulong guildId, string reason, params ulong[] userIds);
        ValueTask KickAsync(ulong guildId, ulong userId, string reason);
        ValueTask PruneAsync(int amount = 5);
        ValueTask MuteAsync(ulong guildId, ulong userId, TimeSpan? duration = null, string reason = null);
        ValueTask UnmuteAsync(ulong guildId, ulong userId);
        ValueTask WarnAsync(ulong guildId, ulong userId, string reason);
        ValueTask UnWarnAsync(ulong guildId, ulong userId, Guid id);
        ValueTask WarnLogAsync(ulong guildId, ulong userId);
        ValueTask ReasonAsync(ulong guildId, int id, string reason);
    }
}