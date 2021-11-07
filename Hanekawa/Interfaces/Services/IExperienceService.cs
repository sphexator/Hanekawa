using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IExperienceService
    {
        ValueTask<int> AddExpAsync(ulong guildId, ulong userId, int amount);
        ValueTask<int> GetExpAsync(ulong guildId, ulong userId);
        ValueTask<int> GetLevelAsync(int level);
        ValueTask<int> GetExpToNextLevelAsync(ulong userId);
        ValueTask<int> GetExpToNextLevelAsync(int level);
        ValueTask DecayAsync();
    }
}