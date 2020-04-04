using System;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public abstract class HanekawaAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context) 
            => CheckAsync((HanekawaContext)context, context.ServiceProvider);

        public abstract ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider);
    }
}