using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Preconditions
{
    public class RequiredChannel : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("Wrong command context.");
            var roles = context.Author.GetRoles();
            if (Disqord.Discord.Permissions.CalculatePermissions(context.Guild, context.Author, roles.Values).Has(Permission.ManageGuild))
                return CheckResult.Successful;
            var cache = context.Services.GetRequiredService<CacheService>();
            var ignoreAll = cache.TryGetIgnoreChannel(context.GuildId, out var status);
            if (!ignoreAll) status = await cache.UpdateIgnoreAllStatus(context);

            var pass = status ? cache.EligibleChannel(context, true) : cache.EligibleChannel(context);

            return pass switch
            {
                true => CheckResult.Successful,
                false => CheckResult.Failed("Cannot execute this command in this channel.")
            };
        }
    }
}
