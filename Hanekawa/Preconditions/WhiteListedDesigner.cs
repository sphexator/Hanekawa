using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Hanekawa.Addons.Database;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WhiteListedDesigner : RequireContextAttribute
    {
        public WhiteListedDesigner() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (!(context.User is IGuildUser user)) return PreconditionResult.FromError("not in a guild");
            if (user.GuildPermissions.ManageGuild) return PreconditionResult.FromSuccess();
            using (var db = new DbService())
            {
                var check = await db.WhitelistDesigns.FindAsync(context.Guild.Id, context.User.Id);
                return check == null
                    ? PreconditionResult.FromError("Not whitelisted")
                    : PreconditionResult.FromSuccess();
            }
        }
    }
}