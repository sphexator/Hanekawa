using System;
using System.Threading.Tasks;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Preconditions
{
    public class RequirePremium : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("wrong right context.");
            if (context.Guild == null) return CheckResult.Failed("Needs to be executed in a guild!");
            using var scope = context.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(context.Guild);
            return cfg.Premium.HasValue && cfg.Premium.Value >= DateTimeOffset.UtcNow
                ? CheckResult.Successful
                : CheckResult.Failed("Command is only available for premium servers!");
        }
    }
}
