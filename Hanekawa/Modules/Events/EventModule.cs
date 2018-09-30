using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Events;
using Hanekawa.Services.Level;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Modules.Events
{
    [Group("Event")]
    [RequireContext(ContextType.Guild)]
    [Summary("Event scheduler. Add, remove or manage scheduled events for your server.")]
    public class EventModule : InteractiveBase
    {
        private readonly EventService _service;
        private readonly LevelingService _levelingService;

        public EventModule(EventService service, LevelingService levelingService)
        {
            _service = service;
            _levelingService = levelingService;
        }

        [Command("post", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task PostEventsAsync()
        {
            await _service.Execute();
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all upcoming events")]
        [Priority(1)]
        [WhiteListedOverAll]
        public async Task ListDesignerEventsAsync()
        {
            using (var db = new DbService())
            {
                var events = await db.EventSchedules
                    .Where(x => x.GuildId == Context.Guild.Id && x.Time >= DateTime.UtcNow).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < events.Count;)
                {
                    string eventString = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == events.Count) continue;
                        var sEvent = events[i];
                        var host = Context.Guild.GetUser(sEvent.Host).Mention ?? "Couldn't find user or left server.";
                        var designer = sEvent.DesignerClaim.HasValue ? Context.Guild.GetUser(sEvent.DesignerClaim.Value).Mention : "Available";
                        var image = sEvent.ImageUrl ?? "No Image";
                        eventString += $"**{sEvent.Name} (ID:{sEvent.Id}**\n" +
                                       $"Date: {sEvent.Time}\n" +
                                      $"Designer: {designer}\n" +
                                       $"Image: {image}" +
                                      $"Host {host}\n";
                        i++;
                    }
                    pages.Add(eventString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Events in {Context.Guild.Name}",
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

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all upcoming events")]
        [RequiredChannel]
        public async Task ListEventsAsync()
        {
            using (var db = new DbService())
            {
                var events = await db.EventSchedules
                    .Where(x => x.GuildId == Context.Guild.Id && x.Time >= DateTime.UtcNow).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < events.Count;)
                {
                    string eventString = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == events.Count) continue;
                        var sEvent = events[i];
                        var host = Context.Guild.GetUser(sEvent.Host).Mention ?? "Couldn't find user or left server.";
                        eventString += $"**{sEvent.Name} (ID:{sEvent.Id}**\n" +
                                       $"Date: {sEvent.Time}\n" +
                                      $"Host {host}\n";
                        i++;
                    }
                    pages.Add(eventString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Events in {Context.Guild.Name}",
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

        [Command("schedule", RunMode = RunMode.Async)]
        [Summary("Sets event scheduling channel")]
        [RequireOwner]
        public async Task SetSchedulingChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.EventSchedulerChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Removed event scheduling channel.", Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.EventSchedulerChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set {channel.Mention} as event scheduling channel", Color.Green.RawValue).Build());
                }
            }
        }

        [Command("channel", RunMode = RunMode.Async)]
        [Summary("Sets event channel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEventChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.EventChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Removed event channel.", Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.EventChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set {channel.Mention} as event channel", Color.Green.RawValue).Build());
                }
            }
        }

        [Command("exp")]
        [Summary("Displays current on-going exp events if there is one.")]
        [RequiredChannel]
        public async Task ShowExpEvent()
        {
            using (var db = new DbService())
            {
                var expEvent = await db.LevelExpEvents.FindAsync(Context.Guild.Id);
                if (expEvent == null)
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("There's currently no event active.").Build());
                }
                else
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"There's currently an exp event active for {(expEvent.Time - DateTime.UtcNow).Humanize()}")
                            .Build());
                }
            }
        }
        
        [Command("Add", RunMode = RunMode.Async)]
        [Summary("Adds a event given the datetime it'll appear (time is in UTC!)")]
        [WhiteListedEventOrg]
        public async Task AddEventAsync()
        {
            var currentTime = DateTime.UtcNow;
            string name = null;
            var day = 1;
            var month = 1;
            string time = null;
            string timezone = null;
            var year = 1;
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Name of event?").Build());
            name = (await NextMessageAsync(true, true, TimeSpan.FromMinutes(5))).Content;
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Which day is this event? (In numbers: 1-31)").Build());
            day = Convert.ToInt32((await NextMessageAsync(true, true, TimeSpan.FromMinutes(5))).Content);
            if (currentTime.Day >= day)
            {
                if (currentTime.Month == 12) month = 1;
                else month = currentTime.Month + 1;
            }
            else month = currentTime.Month;

            if (currentTime.Month > month) year = currentTime.Year + 1;
            else year = currentTime.Year;
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Timezone? (eg. UTC-5)").Build());
            timezone = (await NextMessageAsync(true, true, TimeSpan.FromMinutes(5))).Content;
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Time? (eg. 15:00)").Build());
            time = (await NextMessageAsync(true, true, TimeSpan.FromMinutes(5))).Content;

            var parseCheck = DateTime.TryParse($"{year}-{month}-{day} {time} {timezone}", out var date);
            if (!parseCheck)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Couldn't parse input into a timestamp", Color.Red.RawValue).Build());
                return;
            }

            using (var db = new DbService())
            {
                if (date == DateTime.UtcNow.Date)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Can't schedule an event on the same day.", Color.Red.RawValue)
                            .Build());
                var check = await db.EventSchedules.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.Time == date);
                if (check != null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply("There is already an event scheduled at that time.", Color.Red.RawValue).Build());
                    return;
                }

                var id = await db.EventSchedules.CountAsync(x => x.GuildId == Context.Guild.Id + 1);
                var embed = new EmbedBuilder
                {
                    Title = name,
                    Timestamp = new DateTimeOffset(date),
                    Color = Color.Purple
                };

                embed.AddField("Host", Context.User.Mention ?? ":KuuThinking:");
                await ReplyAsync("Does this look correct? (y/n)", false, embed.Build());
                try
                {
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (response.Content.ToLower() == "n")
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply(
                                    "Cancelling event scheduling. Restart with new designated name/time (time is in UTC)")
                                .Build());
                        return;
                    }

                    if (response.Content.ToLower() == "y")
                    {
                        await _service.TryAddEventAsync(db, name, Context.User as IGuildUser, date);
                        var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply(
                                    $"Scheduled {name} for {time.Humanize()} \nUse `event desc {id} <description>` to add a description to your event\nUse `event image {id} <imageUrl>` to add a image to your event!")
                                .Build());
                        if (cfg.DesignChannel.HasValue)
                        {
                            await Context.Guild.GetTextChannel(cfg.DesignChannel.Value)
                                .SendMessageAsync($"New event added\nClaim to make a banner for this with `!event claim {id}`", false, embed.Build());
                        }
                    }
                }
                catch(Exception e) { Console.WriteLine(e);}
            }
        }
        
        [Command("Remove", RunMode = RunMode.Async)]
        [Summary("Removes a event from the list given the ID")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveEventAsync(int id)
        {
            using (var db = new DbService())
            {
                var eventData =
                    await db.EventSchedules.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Id == id);
                if (eventData == null) return;
                if (eventData.Host != Context.User.Id || !(Context.User as IGuildUser).GuildPermissions.ManageGuild)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You do not own or have the permission to remove events!",
                            Color.Red.RawValue).Build());
                    return;
                }

                await _service.TryRemoveEventAsync(db, id, Context.Guild);
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {eventData.Name} from the event schedule.", Color.Green.RawValue)
                        .Build());
            }
        }

        [Command("claim", RunMode = RunMode.Async)]
        [Summary("Claims a event to make a image for it")]
        [WhiteListedDesigner]
        public async Task ClaimEventAsync(int id)
        {
            using (var db = new DbService())
            {
                var eventData =
                    await db.EventSchedules.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Id == id);
                if (eventData == null) return;
                if (eventData.DesignerClaim.HasValue)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"{Context.User.Mention}, this event is already claimed",
                            Color.Red.RawValue).Build());
                    return;
                }
                eventData.DesignerClaim = Context.User.Id;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"{Context.User.Mention} has claimed to make a banner for {eventData.Name}", Color.Green.RawValue)
                        .Build());
            }
        }

        [Command("Description", RunMode = RunMode.Async)]
        [Alias("desc")]
        [Summary("Sets a description to the event")]
        [WhiteListedEventOrg]
        public async Task SetEventDescriptionAsync(int id, [Remainder] string content)
        {
            using (var db = new DbService())
            {
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventData == null) return;
                eventData.Description = content;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Updated description for {eventData.Name}", Color.Green.RawValue)
                        .Build());
            }
        }

        [Command("Image", RunMode = RunMode.Async)]
        [Alias("img")]
        [Summary("Sets a image to the event")]
        [WhiteListedOverAll]
        public async Task SetEventImageUrlAsync(int id, string url)
        {
            using (var db = new DbService())
            {
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventData == null) return;
                if (!eventData.DesignerClaim.HasValue) eventData.DesignerClaim = Context.User.Id;
                eventData.ImageUrl = url;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Updated event image for {eventData.Name}", Color.Green.RawValue)
                        .Build());
            }
        }
    }
}