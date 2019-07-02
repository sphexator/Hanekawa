using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Shared.Interactive.Criteria;

namespace Hanekawa.Shared.Interactive.Paginator
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