using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IAutoModerationService
    {
        ValueTask MessageReceivedAsync();
        ValueTask MessageUpdatedAsync();
        ValueTask UserJoinedAsync();
    }
}