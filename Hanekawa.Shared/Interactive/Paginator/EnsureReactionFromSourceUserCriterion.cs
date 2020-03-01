using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive.Criteria;

namespace Hanekawa.Shared.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, SocketReaction parameter) 
            => Task.FromResult(parameter.UserId == sourceContext.User.Id);

        public Task<bool> JudgeAsync(ulong? channelId, ulong? userId, SocketReaction parameter) 
            => !userId.HasValue 
                ? Task.FromResult(false) 
                : Task.FromResult(parameter.UserId == userId.Value);
    }
}