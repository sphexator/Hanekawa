using System;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public class GuildOnlyAttribute : HanekawaAttribute
    {
        public GuildOnlyAttribute()
        { }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider) 
            => context.Guild != null
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This can only be executed in a guild.");
    }
}