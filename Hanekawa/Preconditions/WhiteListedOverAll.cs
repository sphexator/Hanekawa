using System;
using System.Threading.Tasks;
using Discord.Commands;
using Hanekawa.Services.Entities;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WhiteListedOverAll : RequireContextAttribute
    {
        public WhiteListedOverAll() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            using (var db = new DbService())
            {
                var designer = await db.WhitelistDesigns.FindAsync(context.Guild.Id, context.User.Id);
                var eventOrg = await db.WhitelistEvents.FindAsync(context.Guild.Id, context.User.Id);
                if (designer == null && eventOrg == null) return PreconditionResult.FromError("Not whitelisted");
                return PreconditionResult.FromSuccess();
            }
        }
    }
}