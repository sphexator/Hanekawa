using System;
using System.Threading.Tasks;
using Hanekawa.Entities.Config;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.WebUI.Bot.Commands.Preconditions
{
    public class RequirePremium : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("Wrong context.");
            await using var db = context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<GuildConfig>(context.GuildId);
            return cfg.Premium.HasValue && cfg.Premium.Value >= DateTimeOffset.UtcNow
                ? CheckResult.Successful
                : CheckResult.Failed("Command is only available for premium servers.");
        }
    }
}
