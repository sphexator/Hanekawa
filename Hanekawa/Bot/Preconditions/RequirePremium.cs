using System.Threading.Tasks;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    public class RequirePremium : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (!(_ is DiscordCommandContext context)) return CheckResult.Unsuccessful("wrong right context.");
            if(context.Guild == null) return CheckResult.Unsuccessful("Needs to be executed in a guild!");
            using var scope = context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(context.Guild);
            return cfg.Premium 
                ? CheckResult.Successful 
                : CheckResult.Unsuccessful("Command is only available for premium servers!");
        }
    }
}
