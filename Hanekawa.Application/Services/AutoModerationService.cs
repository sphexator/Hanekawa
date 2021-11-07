using System.Threading.Tasks;
using Hanekawa.Interfaces.Services;

namespace Hanekawa.Application.Services
{
    public class AutoModerationService : IAutoModerationService
    {
        public ValueTask MessageReceivedAsync()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask MessageUpdatedAsync()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask UserJoinedAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}