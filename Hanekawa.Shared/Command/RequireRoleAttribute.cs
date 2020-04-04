using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireRoleAttribute : GuildOnlyAttribute
    {
        public Snowflake Id { get; }

        public RequireRoleAttribute(ulong id) => Id = id;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            return context.Member.Roles.ContainsKey(Id)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"You do not have the required role {Id}.");

        }
    }
}
