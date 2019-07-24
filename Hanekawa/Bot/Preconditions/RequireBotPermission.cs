using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireBotPermission : HanekawaAttribute
    {
        private readonly GuildPermission[] _perms;

        public RequireBotPermission(params GuildPermission[] perms) => _perms = perms;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            for (var i = 0; i < _perms.Length; i++)
                if (!context.Guild.CurrentUser.GuildPermissions.Has(_perms[i]))
                    return CheckResult.Unsuccessful($"Bot needs {_perms[i]} guild permission");

            return CheckResult.Successful;
        }
    }
}