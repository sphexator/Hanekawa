using System;
using System.Threading.Tasks;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireOwner : HanekawaAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider) =>
            context.User.Id == 111123736660324352
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command only usable by bot owner");
    }
}
