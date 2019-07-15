using System.Threading.Tasks;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(HanekawaContext sourceContext, T parameter);
        Task<bool> JudgeAsync(ulong? channelId, ulong? userId, T parameter);
    }
}