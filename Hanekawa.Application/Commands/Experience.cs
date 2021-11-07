using System;
using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Experience : IExperienceCommands
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

        public ValueTask<int> GetExperienceAsync(ulong guildId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask CreateEventAsync(ulong guildId, string eventName, string eventDescription, DateTime eventDate)
        {
            throw new NotImplementedException();
        }

        public ValueTask ListEventAsync(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveEventAsync(ulong guildId)
        {
            throw new NotImplementedException();
        }
    }
}