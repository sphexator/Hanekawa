using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interactive.Criteria;
using Qmmands;

namespace Hanekawa.Core.Interactive.Callbacks
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