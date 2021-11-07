using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IBoostCommands
    {
        ValueTask SetExpRewardAsync(ulong guildId, int exp);
        ValueTask SetCreditRewardAsync(ulong guildId, int credit);
        ValueTask SetSpecialCreditRewardAsync(ulong guildId, int specialCredit);
        ValueTask SetExpMultiplierAsync(ulong guildId, double multiplier);
        ValueTask SetCreditMultiplierAsync(ulong guildId, double multiplier);
        ValueTask SetSpecialCreditMultiplierAsync(ulong guildId, double multiplier);
        ValueTask SetAnnouncementChannelAsync(ulong guildId, ulong channelId);
        ValueTask ForceAsync(ulong guildId);
    }
}