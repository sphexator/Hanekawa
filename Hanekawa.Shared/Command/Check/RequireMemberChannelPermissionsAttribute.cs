using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireMemberChannelPermissionsAttribute : GuildOnlyAttribute
    {
        public ChannelPermissions Permissions { get; }

        public RequireMemberChannelPermissionsAttribute(Permission permissions) => Permissions = permissions;

        public RequireMemberChannelPermissionsAttribute(params Permission[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            Permissions = permissions.Aggregate(Permission.None, (total, permission) => total | permission);
        }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            var permissions = context.Member.GetPermissionsFor(context.Channel as IGuildChannel);
            return permissions.Has(Permissions)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"You lack the necessary channel permissions ({Permissions - permissions}) to execute this.");
        }
    }
}