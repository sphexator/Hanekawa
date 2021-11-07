using System;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IExperienceCommands : ICommandSettings
    {
        ValueTask<int> GetExperienceAsync(ulong guildId, ulong userId);
        ValueTask CreateEventAsync(ulong guildId, string eventName, string eventDescription, DateTime eventDate);
        ValueTask ListEventAsync(ulong guildId);
        ValueTask RemoveEventAsync(ulong guildId);
    }
}