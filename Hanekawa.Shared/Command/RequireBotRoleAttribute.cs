using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireBotRoleAttribute : GuildOnlyAttribute
    {
        public Snowflake Id { get; }

        public RequireBotRoleAttribute(ulong id) => Id = id;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            return context.Guild.CurrentMember.Roles.ContainsKey(Id)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The bot does not have the required role {Id}.");

        }
    }
}
