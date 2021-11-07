using System;
using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Mvp : IMvpCommands
    {
        public ValueTask<T> SetOrAddAsync<T>()
        {
            throw new NotImplementedException();
        }

        public ValueTask<T> DisableOrRemoveAsync<T>()
        {
            throw new NotImplementedException();
        }

        public ValueTask<T> ListOrGetAsync<T>()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> OptOutAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask SetMvpDayAsync(DayOfWeek day, TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> ExperienceRewardAsync(int experience)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> CreditRewardAsync(int credits)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> SpecialCreditRewardAsync(int credits)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> ForceAsync()
        {
            throw new NotImplementedException();
        }
    }
}