using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Modules.Permission
{

    [Group("automoderator")]
    [Alias("automod")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class FilterPermission : InteractiveBase
    {
        [Command("invite")]
        [Alias("srvfilter")]
        [Summary("Toggles guild invite filter, auto-deletes invites")]
        public async Task InviteFilter()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.FilterInvites)
                {
                    cfg.FilterInvites = false;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply("Disabled auto deletion and muting users posting invite links.",
                                Color.Green.RawValue).Build());
                    return;
                }

                cfg.FilterInvites = true;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Enabled auto deletion and muting users posting invite links.",
                        Color.Green.RawValue).Build());
            }
        }
    }
}