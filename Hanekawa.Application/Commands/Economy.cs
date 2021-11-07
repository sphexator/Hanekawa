using System.Collections.Generic;
using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Economy : IEconomyCommands
    {
        public ValueTask<int> BalanceAsync(ulong userId)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<int> AddAsync(ulong userId, int amount)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<int> RemoveAsync(ulong userId, int amount)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<List<(ulong, int)>> GetTopAsync(int page, int count = 50)
        {
            throw new System.NotImplementedException();
        }
    }
}