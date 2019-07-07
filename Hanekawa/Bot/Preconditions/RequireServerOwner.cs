using System;
using System.Threading.Tasks;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireServerOwner : HanekawaAttribute
    {
        public RequireServerOwner()  { }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider) 
            => context.Guild.OwnerId == context.User.Id 
                ? CheckResult.Successful 
                : CheckResult.Unsuccessful("Command only usable by server owners");
    }
}