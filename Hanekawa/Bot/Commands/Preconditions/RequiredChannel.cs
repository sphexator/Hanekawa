using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Preconditions
{
    public class RequiredChannel : CheckAttribute, INService
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("Wrong command context.");
            var roles = context.Author.GetRoles();
            if (Discord.Permissions.CalculatePermissions(context.Guild, context.Author, roles.Values).Has(Permission.ManageGuild))
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
