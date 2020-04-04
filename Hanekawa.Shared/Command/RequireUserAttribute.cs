using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireUserAttribute : HanekawaAttribute
    {
        public Snowflake Id { get; }

        public RequireUserAttribute(ulong id) => Id = id;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider) =>
            Id == context.User.Id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("You are not authorized to execute this.");
    }
}
