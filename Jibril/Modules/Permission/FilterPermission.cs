using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Modules.Permission
{
    [Group("automoderator")]
    [Alias("automod")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class FilterPermission : InteractiveBase
    {
        [Command("Setup")]
        [Summary("Go through a setup process to configure auto-moderator.")]
        [RequireOwner]
        public async Task AutoModSetupAsync()
        {
            using(var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                await ReplyAsync("Setting up auto-moderator. If you already have configured it beforehand, this'll overwrite those settings.\n\n Filter all discord invite links?(y/n)");
                bool filterStatus = true;
                while(filterStatus)
                {
                    var response = await NextMessageAsync();
                    if(response.Content.ToLower() == "y"){
                        cfg.FilterInvites = true;
                        filterStatus = false;
                        }
                    if(response.Content.ToLower() == "n")
                    {
                        cfg.FilterInvites = false;
                        filterStatus = false;
                    }
                }
            }
        }

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