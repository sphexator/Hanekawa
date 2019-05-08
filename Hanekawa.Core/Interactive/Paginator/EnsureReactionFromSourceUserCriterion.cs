using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interactive.Criteria;

namespace Hanekawa.Core.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, SocketReaction parameter)
        {
            var ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}