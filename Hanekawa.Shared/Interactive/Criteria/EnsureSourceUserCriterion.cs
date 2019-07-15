using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}