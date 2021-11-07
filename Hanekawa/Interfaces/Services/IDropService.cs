using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IDropService
    {
        ValueTask<bool> CreateDropAsync(ulong channelId, bool special);
        ValueTask<bool> DeleteDropAsync(ulong channelId, ulong messageId);
        ValueTask MessageReceivedAsync(ulong channelId, ulong messageId);
    }
}