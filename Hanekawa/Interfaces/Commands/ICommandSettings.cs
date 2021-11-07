using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface ICommandSettings
    {
        ValueTask<T> SetOrAddAsync<T>();
        ValueTask<T> DisableOrRemoveAsync<T>();
        ValueTask<T> ListOrGetAsync<T>();
    }
}