using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Core;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireUserPermission : HanekawaAttribute
    {
        private readonly GuildPermission[] _perms;

        public RequireUserPermission(params GuildPermission[] perms)
        {
            _perms = perms;
        }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            for (var i = 0; i < _perms.Length; i++)
                if (!context.User.GuildPermissions.Has(_perms[i]))
                    return CheckResult.Unsuccessful($"User needs {_perms[i]} guild permission");
            return CheckResult.Successful;
        }
    }
}