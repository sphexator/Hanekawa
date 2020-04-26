
using System;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class GuildOwnerOnlyAttribute : GuildOnlyAttribute
    {
        public GuildOwnerOnlyAttribute()
        { }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            var baseResult = base.CheckAsync(context).Result;
            return !baseResult.IsSuccessful
                ? baseResult
                : (ValueTask<CheckResult>)(context.User.Id.RawValue == context.Guild.OwnerId
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This can only be executed by the guild's owner."));
        }
    }
}
