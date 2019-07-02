using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Shared.Interactive.Criteria;
using Hanekawa.Shared.Interactive.Paginator;
using Hanekawa.Shared.Interactive.Results;
using Qmmands;

namespace Hanekawa.Shared.Interactive
{
    public abstract class InteractiveBase : InteractiveBase<HanekawaContext>
    {
    }

    public abstract class InteractiveBase<T> : ModuleBase<T>
        where T : HanekawaContext
    {
        public InteractiveService Interactive { get; set; }

        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            return Interactive.NextMessageAsync(Context, criterion, timeout, token);
        }

        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true,
            TimeSpan? timeout = null, CancellationToken token = default)
        {
            return Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout, token);
        }

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, Embed embed = null,
            TimeSpan? timeout = null, RequestOptions options = null)
        {
            return Interactive.ReplyAndDeleteAsync(Context, content, isTTS, embed, timeout, options);
        }

        public Task<IUserMessage> PagedReplyAsync(IEnumerable<object> pages, bool fromSourceUser = true)
        {
            var pager = new PaginatedMessage
            {
                Pages = pages
            };
            return PagedReplyAsync(pager, fromSourceUser);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            return PagedReplyAsync(pager, criterion);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketReaction> criterion)
        {
            return Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        }

        public CommandResult Ok(string reason = null)
        {
            return new OkResult(reason);
        }
    }
}