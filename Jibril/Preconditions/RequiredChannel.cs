using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredChannel : RequireContextAttribute
    {
        private readonly ulong _requiredChannel;

        public RequiredChannel(ulong requiredChannel) : base(ContextType.Guild)
        {
            _requiredChannel = requiredChannel;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (baseResult.IsSuccess && ((IGuildChannel) context.Channel).Id == _requiredChannel)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("Wrong channel");
        }
    }
}