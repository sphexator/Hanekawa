using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireUserPermission : HanekawaAttribute
    {
        public readonly GuildPermission[] Perms;

        public RequireUserPermission(params GuildPermission[] perms) => Perms = perms;

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            for (var i = 0; i < Perms.Length; i++)
                if (!context.User.GuildPermissions.Has(Perms[i]))
                    return CheckResult.Unsuccessful($"User needs {Perms[i]} guild permission");
            return CheckResult.Successful;
        }
    }
}