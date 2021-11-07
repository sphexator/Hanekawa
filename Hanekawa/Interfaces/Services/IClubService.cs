using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Services
{
    public interface IClubService
    {
        ValueTask CreateAsync();
        ValueTask AddAsync();
        ValueTask RemoveAsync();
        ValueTask DemoteAsync();
        ValueTask PromoteAsync();
        ValueTask UpdateAsync();
    }
}