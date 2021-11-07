using System;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IMvpCommands : ICommandSettings
    {
        ValueTask<bool> OptOutAsync();
        ValueTask SetMvpDayAsync(DayOfWeek day, TimeSpan time);
        ValueTask<int> ExperienceRewardAsync(int experience);
        ValueTask<int> CreditRewardAsync(int credits);
        ValueTask<int> SpecialCreditRewardAsync(int credits);
        ValueTask<bool> ForceAsync();
    }
}