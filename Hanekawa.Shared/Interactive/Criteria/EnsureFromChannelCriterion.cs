using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureFromChannelCriterion : ICriterion<IMessage>
    {
        private readonly ulong _channelId;

        public EnsureFromChannelCriterion(IMessageChannel channel) => _channelId = channel.Id;

        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter)
            => Task.FromResult(_channelId == parameter.Channel.Id);
    }
}