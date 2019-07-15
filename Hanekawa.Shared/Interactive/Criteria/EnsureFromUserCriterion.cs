using System.ComponentModel;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureFromUserCriterion : ICriterion<IMessage>
    {
        private readonly ulong _id;

        public EnsureFromUserCriterion(IUser user) => _id = user.Id;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public EnsureFromUserCriterion(ulong id) => _id = id;

        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter) =>
            Task.FromResult(_id == parameter.Author.Id);

        public Task<bool> JudgeAsync(ulong? channelId, ulong? userId, IMessage parameter) => !userId.HasValue
            ? Task.FromResult(false)
            : Task.FromResult(userId.Value == parameter.Author.Id);
    }
}