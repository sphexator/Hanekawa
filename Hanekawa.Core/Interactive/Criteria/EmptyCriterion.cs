using System.Threading.Tasks;

namespace Hanekawa.Core.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, T parameter)
        {
            return Task.FromResult(true);
        }
    }
}