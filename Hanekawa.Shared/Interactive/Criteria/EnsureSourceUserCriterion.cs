using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter) =>
            Task.FromResult(sourceContext.User.Id == parameter.Author.Id);

        public Task<bool> JudgeAsync(ulong? channelId, ulong? userId, IMessage parameter) => !userId.HasValue
            ? Task.FromResult(false)
            : Task.FromResult(userId.Value == parameter.Author.Id);
    }
}