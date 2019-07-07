using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive.Criteria;
using Qmmands;

namespace Hanekawa.Shared.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        HanekawaContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}