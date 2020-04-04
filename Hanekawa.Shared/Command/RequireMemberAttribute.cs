using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireMemberAttribute : GuildOnlyAttribute
    {
        public Snowflake Id { get; }

        public RequireMemberAttribute(ulong id) => Id = id;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            return Id == context.Member.Id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("You are not authorized to execute this.");
        }
    }
}
