using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Welcome : IWelcomeCommands
    {
        public ValueTask<T> SetOrAddAsync<T>()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<T> DisableOrRemoveAsync<T>()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<T> ListOrGetAsync<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}