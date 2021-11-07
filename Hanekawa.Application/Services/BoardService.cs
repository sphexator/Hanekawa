using System.Threading.Tasks;
using Hanekawa.Interfaces.Services;

namespace Hanekawa.Application.Services
{
    public class BoardService : IBoardService
    {
        public ValueTask<bool> PostAsync(ulong channelId, string message)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask ReactionReceivedAsync()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask ReactionRemovedAsync()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask ReactionsClearedAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}