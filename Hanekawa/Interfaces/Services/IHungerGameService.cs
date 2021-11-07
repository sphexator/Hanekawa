using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IHungerGameService
    {
        ValueTask PostAsync();
        
    }
}