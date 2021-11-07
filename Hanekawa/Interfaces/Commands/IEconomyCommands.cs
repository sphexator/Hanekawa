using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IEconomyCommands
    {
        ValueTask<int> BalanceAsync(ulong userId);
        ValueTask<int> AddAsync(ulong userId, int amount);
        ValueTask<int> RemoveAsync(ulong userId, int amount);
        ValueTask<List<(ulong, int)>> GetTopAsync(int page, int count = 50);
    }
}