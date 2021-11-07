using System.Threading.Tasks;
using Hanekawa.Interfaces.Services;

namespace Hanekawa.Application.Services
{
    public class DropService : IDropService
    {
        public ValueTask<bool> CreateDropAsync(ulong channelId, bool special)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<bool> DeleteDropAsync(ulong channelId, ulong messageId)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask MessageReceivedAsync(ulong channelId, ulong messageId)
        {
            throw new System.NotImplementedException();
        }
    }
}