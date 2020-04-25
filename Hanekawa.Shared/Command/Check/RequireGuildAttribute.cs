using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireGuildAttribute : GuildOnlyAttribute
    {
        public Snowflake Id { get; }

        public RequireGuildAttribute(ulong id) => Id = id;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            return Id == context.Guild.Id
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This cannot be executed in this guild.");
        }
    }
}