using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Premium
{
    [Name("Hunger Games")]
    [Description("")]
    [RequirePremium]
    public class HungerGame : HanekawaCommandModule
    {
        [Name("Set Channel")]
        [Description("Sets channel to output Hunger Game events")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetChannelAsync(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.HungerGameChannel = null;
                await Context.ReplyAsync("Turned off Hunger Games!");
            }
            else
            {
                cfg.HungerGameChannel = channel.Id.RawValue;
                await Context.ReplyAsync($"Enabled or changed Hunger Games channel to {channel.Mention}");
            }

            await db.SaveChangesAsync();
        }

        [Name("Set Reward Role")]
        [Description("Sets role that's rewarded to the winner of hunger games")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetRoleAsync(CachedRole role = null)
        {

        }
    }
}