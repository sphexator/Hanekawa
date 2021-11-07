using System;
using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Administration : IAdministrationCommands
    {
        public ValueTask BanAsync(ulong guildId, ulong userId, string reason)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnbanAsync(ulong guildId, ulong userId, string reason)
        {
            throw new NotImplementedException();
        }

        public ValueTask MassBanAsync(ulong guildId, string reason, params ulong[] userIds)
        {
            throw new NotImplementedException();
        }

        public ValueTask KickAsync(ulong guildId, ulong userId, string reason)
        {
            throw new NotImplementedException();
        }

        public ValueTask PruneAsync(int amount = 5)
        {
            throw new NotImplementedException();
        }

        public ValueTask MuteAsync(ulong guildId, ulong userId, TimeSpan? duration = null, string reason = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnmuteAsync(ulong guildId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask WarnAsync(ulong guildId, ulong userId, string reason)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnWarnAsync(ulong guildId, ulong userId, Guid id)
        {
            throw new NotImplementedException();
        }

        public ValueTask WarnLogAsync(ulong guildId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask ReasonAsync(ulong guildId, int id, string reason)
        {
            throw new NotImplementedException();
        }
    }
}