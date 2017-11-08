using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequiredChannel : RequireContextAttribute
    {
        private readonly ulong _requiredChannel;

        public RequiredChannel(ulong requiredChannel) : base(ContextType.Guild)
        {
            _requiredChannel = requiredChannel;
        }

        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (baseResult.IsSuccess && ((IGuildChannel)context.Channel).Id == _requiredChannel)
            {
                return PreconditionResult.FromSuccess();
            }
            else
            {
                return PreconditionResult.FromError("Wrong channel");
            }
        }
    }
}
