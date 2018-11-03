using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Administration;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Whitelist : InteractiveBase
    {
        [Command("event add", RunMode = RunMode.Async)]
        public async Task Eventadd(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistEvents.FindAsync(Context.Guild.Id, Context.User.Id);
                if (check != null)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("User is already a whitelisted event organiser.").Build());

                var data = new WhitelistEvent
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id
                };
                await db.WhitelistEvents.AddAsync(data);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {user.Mention} as whitelisted event organiser!").Build());
            }
        }

        [Command("event remove", RunMode = RunMode.Async)]
        public async Task EventRemove(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistEvents.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id);
                if (check == null)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("User isn't whitelisted!", Color.Red.RawValue).Build());

                db.WhitelistEvents.Remove(check);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {user.Mention} as whitelisted event organiser!").Build());
            }
        }

        [Command("event list", RunMode = RunMode.Async)]
        public async Task EventList(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var events = await db.WhitelistEvents
                    .Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < events.Count;)
                {
                    string eventString = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == events.Count) continue;
                        var sEvent = events[i];
                        eventString += $"{Context.Guild.GetUser(sEvent.UserId).Mention ?? "User left server"}\n";
                        i++;
                    }

                    pages.Add(eventString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Whitelisted event organisers in {Context.Guild.Name}",
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
            }
        }

        [Command("design add", RunMode = RunMode.Async)]
        public async Task DesignAdd(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistDesigns.FindAsync(Context.Guild.Id, Context.User.Id);
                if (check != null)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("User is already a whitelisted designer.").Build());

                var data = new WhitelistDesign
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id
                };
                await db.WhitelistDesigns.AddAsync(data);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {user.Mention} as whitelisted designer!").Build());
            }
        }

        [Command("design remove", RunMode = RunMode.Async)]
        public async Task DesignRemove(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistDesigns.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id);
                if (check == null)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("User isn't whitelisted!", Color.Red.RawValue).Build());

                db.WhitelistDesigns.Remove(check);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {user.Mention} as whitelisted designer!").Build());
            }
        }

        [Command("design list", RunMode = RunMode.Async)]
        public async Task DesignList()
        {
            using (var db = new DbService())
            {
                var designers = await db.WhitelistDesigns
                    .Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < designers.Count;)
                {
                    string eventString = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == designers.Count) continue;
                        var sEvent = designers[i];
                        eventString += $"{Context.Guild.GetUser(sEvent.UserId).Mention ?? "User left server"}\n";
                        i++;
                    }

                    pages.Add(eventString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Whitelisted designers in {Context.Guild.Name}",
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
            }
        }
    }
}