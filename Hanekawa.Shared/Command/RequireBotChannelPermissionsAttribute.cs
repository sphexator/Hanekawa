using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireBotChannelPermissionsAttribute : GuildOnlyAttribute
    {
        public ChannelPermissions Permissions { get; }

        public RequireBotChannelPermissionsAttribute(Permission permissions) => Permissions = permissions;

        public RequireBotChannelPermissionsAttribute(params Permission[] permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            Permissions = permissions.Aggregate(Permission.None, (total, permission) => total | permission);
        }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            var permissions = context.Guild.CurrentMember.GetPermissionsFor(context.Channel);
            return permissions.Has(Permissions)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"The bot lacks the necessary channel permissions ({Permissions - permissions}) to execute this.");
        }
    }
}
