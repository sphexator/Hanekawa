using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireBotGuildPermissionsAttribute : GuildOnlyAttribute
    {
        public GuildPermissions Permissions { get; }

        public RequireBotGuildPermissionsAttribute(Permission permissions) => Permissions = permissions;

        public RequireBotGuildPermissionsAttribute(params Permission[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            Permissions = permissions.Aggregate(Permission.None, (total, permission) => total | permission);
        }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            var permissions = context.Guild.CurrentMember.Permissions;
            return permissions.Has(Permissions)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The bot lacks the necessary guild permissions ({Permissions - permissions}) to execute this.");
        }
    }
}
