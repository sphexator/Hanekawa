using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Shared.Interactive.Criteria;

namespace Hanekawa.Shared.Interactive.Paginator
{
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, SocketMessage parameter)
        {
            var ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}