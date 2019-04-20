using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireServerOwner : RequireContextAttribute
    {
        public RequireServerOwner() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (!(context.User is SocketGuildUser user))
                return PreconditionResult.FromError("Command only usable within a guild");
            if (user.Guild.OwnerId == user.Id) return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("Command only usable by server owners");
        }
    }
}