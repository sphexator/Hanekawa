using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Administration;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Whitelist : InteractiveBase
    {
        [Name("Event add")]
        [Command("event add", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nAdds a user as a whitelisted event organizer")]
        [Remarks("h.event add @bob#0000")]
        public async Task Eventadd(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistEvents.FindAsync(Context.Guild.Id, Context.User.Id);
                if (check != null)
                {
                    await Context.ReplyAsync("User is already a whitelisted event organizer.");
                    return;
                }

                var data = new WhitelistEvent
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id
                };
                await db.WhitelistEvents.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {user.Mention} as whitelisted event organizer!");
            }
        }

        [Name("Event Remove")]
        [Command("event remove", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nRemoves a user from being a whitelisted event organizer")]
        [Remarks("h.event remove @bob#0000")]
        public async Task EventRemove(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistEvents.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id);
                if (check == null)
                {
                    await Context.ReplyAsync("User isn't whitelisted!", Color.Red.RawValue);
                    return;
                }

                db.WhitelistEvents.Remove(check);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {user.Mention} as whitelisted event organizer!");
            }
        }

        [Name("Event list")]
        [Command("event list", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nList all whitelisted event organizers")]
        [Remarks("h.event list")]
        public async Task EventList()
        {
            using (var db = new DbService())
            {
                var events = await db.WhitelistEvents
                    .Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (events.Count == 0)
                {
                    await Context.ReplyAsync("No whitelisted event organizers");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in events)
                    pages.Add($"{Context.Guild.GetUser(x.UserId).Mention ?? "User left server"}\n");

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Whitelisted event organizers in {Context.Guild.Name}"));
            }
        }

        [Name("Design add")]
        [Command("design add", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nAdds a user as a whitelisted designer")]
        [Remarks("h.design add @bob#0000")]
        public async Task DesignAdd(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistDesigns.FindAsync(Context.Guild.Id, Context.User.Id);
                if (check != null)
                {
                    await Context.ReplyAsync("User is already a whitelisted designer.");
                    return;
                }

                var data = new WhitelistDesign
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id
                };
                await db.WhitelistDesigns.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {user.Mention} as whitelisted designer!");
            }
        }

        [Name("Design remove")]
        [Command("design remove", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nRemoves a user from being a whitelisted designer")]
        [Remarks("h.design remove @bob#0000")]
        public async Task DesignRemove(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistDesigns.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id);
                if (check == null)
                {
                    await Context.ReplyAsync("User isn't whitelisted!", Color.Red.RawValue);
                    return;
                }

                db.WhitelistDesigns.Remove(check);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {user.Mention} as whitelisted designer!");
            }
        }

        [Name("Design list")]
        [Command("design list", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nLists all whitelisted designers")]
        [Remarks("h.design list")]
        public async Task DesignList()
        {
            using (var db = new DbService())
            {
                var designers = await db.WhitelistDesigns
                    .Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (designers.Count == 0)
                {
                    await Context.ReplyAsync("No whitelisted designers");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in designers)
                    pages.Add($"{Context.Guild.GetUser(x.UserId).Mention ?? "User left server"}\n");

                await PagedReplyAsync(
                    pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                        $"Whitelisted designers in {Context.Guild.Name}"));
            }
        }
    }
}