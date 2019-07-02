using System.Threading.Tasks;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(HanekawaContext sourceContext, T parameter);
    }
}