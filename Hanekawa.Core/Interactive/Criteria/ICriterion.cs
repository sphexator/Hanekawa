using System.Threading.Tasks;

namespace Hanekawa.Core.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(HanekawaContext sourceContext, T parameter);
    }
}