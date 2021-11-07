using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IBoardService
    {
        ValueTask<bool> PostAsync(ulong channelId, string message);
        ValueTask ReactionReceivedAsync();
        ValueTask ReactionRemovedAsync();
        ValueTask ReactionsClearedAsync();
    }
}