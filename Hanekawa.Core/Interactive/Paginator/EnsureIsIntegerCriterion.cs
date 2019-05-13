using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interactive.Criteria;

namespace Hanekawa.Core.Interactive.Paginator
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