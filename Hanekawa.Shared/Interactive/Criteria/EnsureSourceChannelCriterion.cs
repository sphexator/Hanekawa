using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter) =>
            Task.FromResult(sourceContext.Channel.Id == parameter.Channel.Id);

        public Task<bool> JudgeAsync(ulong? channelId, ulong? userId, IMessage parameter) => !channelId.HasValue
            ? Task.FromResult(false)
            : Task.FromResult(channelId.Value == parameter.Channel.Id);
    }
}