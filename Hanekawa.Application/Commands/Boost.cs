using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Boost : IBoostCommands
    {
        public ValueTask SetExpRewardAsync(ulong guildId, int exp)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetCreditRewardAsync(ulong guildId, int credit)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetSpecialCreditRewardAsync(ulong guildId, int specialCredit)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetExpMultiplierAsync(ulong guildId, double multiplier)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetCreditMultiplierAsync(ulong guildId, double multiplier)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetSpecialCreditMultiplierAsync(ulong guildId, double multiplier)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask SetAnnouncementChannelAsync(ulong guildId, ulong channelId)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask ForceAsync(ulong guildId)
        {
            throw new System.NotImplementedException();
        }
    }
}