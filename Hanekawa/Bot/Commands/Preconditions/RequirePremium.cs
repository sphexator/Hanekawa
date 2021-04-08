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
            if (!(_ is HanekawaCommandContext context)) return CheckResult.Unsuccessful("wrong right context.");
            if (context.Guild == null) return CheckResult.Unsuccessful("Needs to be executed in a guild!");
            using var scope = context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(context.Guild);
            return cfg.Premium
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command is only available for premium servers!");
        }
    }
}
