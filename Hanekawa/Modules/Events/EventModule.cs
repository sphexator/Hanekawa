using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Events;
using Hanekawa.Services.Level;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Events
{
    [Group("Event")]
    [RequireContext(ContextType.Guild)]
    [Summary("Event scheduler. Add, remove or manage scheduled events for your server.")]
    public class EventModule : InteractiveBase
    {
        private readonly LevelingService _levelingService;
        private readonly EventService _service;

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
                if (events.Count == 0)
                {
                    await Context.ReplyAsync("No events scheduled");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in events)
                {
                    var host = Context.Guild.GetUser(x.Host).Mention ?? "Couldn't find user or left server.";
                    var designer = x.DesignerClaim.HasValue
                        ? Context.Guild.GetUser(x.DesignerClaim.Value).Mention
                        : "N/A - Available";
                    var image = x.ImageUrl ?? "No Image";
                    pages.Add($"**{x.Name} (ID:{x.Id})**\n" +
                              $"Date: {x.Time}\n" +
                              $"Designer: {designer}\n" +
                              $"Image: {image}\n" +
                              $"Host {host}\n\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Events in {Context.Guild.Name}"));
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
                if (events.Count == 0)
                {
                    await Context.ReplyAsync("No events scheduled");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in events)
                {
                    var host = Context.Guild.GetUser(x.Host).Mention ?? "Couldn't find user or left server.";
                    pages.Add($"**{x.Name} (ID:{x.Id})**\n" +
                              $"Date: {x.Time}\n" +
                              $"Host {host}\n\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Events in {Context.Guild.Name}"));
            }
        }

        [Command("schedule", RunMode = RunMode.Async)]
        [Summary("Sets event scheduling channel")]
        [RequireOwner]
        public async Task SetSchedulingChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.EventSchedulerChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Removed event scheduling channel.", Color.Green.RawValue);
                }
                else
                {
                    cfg.EventSchedulerChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set {channel.Mention} as event scheduling channel",
                        Color.Green.RawValue);
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
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.EventChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Removed event channel.", Color.Green.RawValue);
                }
                else
                {
                    cfg.EventChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set {channel.Mention} as event channel", Color.Green.RawValue);
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
                    await Context.ReplyAsync("There's currently no event active.");
                else
                    await Context.ReplyAsync(
                        $"There's currently an exp event active for {(expEvent.Time - DateTime.UtcNow).Humanize()}");
            }
        }

        [Command("Add", RunMode = RunMode.Async)]
        [Summary("Adds a event given the datetime it'll appear (time is in UTC!)")]
        [WhiteListedEventOrg]
        public async Task AddEventAsync()
        {
            using (var db = new DbService())
            {
                var currentTime = DateTime.UtcNow;
                string name = null;
                var day = 1;
                var month = 1;
                string time = null;
                string timezone = null;
                var year = 1;

                var nameMsg = await Context.ReplyAsync("Name of event?");
                var nameResponse = await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                name = nameResponse.Content;

                var dayMsg = await Context.ReplyAsync("Which day is this event? (In numbers: 1-31)");
                var dayResponse = await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                day = Convert.ToInt32(dayResponse.Content);

                if (currentTime.Day >= day)
                {
                    if (currentTime.Month == 12)
                        month = 1;
                    else
                        month = currentTime.Month + 1;
                }
                else
                {
                    month = currentTime.Month;
                }

                if (currentTime.Month > month)
                    year = currentTime.Year + 1;
                else
                    year = currentTime.Year;

                var timeMsg = await Context.ReplyAsync("Time? (eg. 15:00)");
                var timeResponse = await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                time = timeResponse.Content;

                var timezoneMsg = await Context.ReplyAsync("Timezone? (eg. -5)");
                var timezoneResponse = await NextMessageAsync(true, true, TimeSpan.FromMinutes(5));
                timezone = timezoneResponse.Content;

                var messages = new List<IMessage>
                {
                    Context.Message,
                    nameMsg,
                    nameResponse,
                    dayMsg,
                    dayResponse,
                    timeMsg,
                    timeResponse,
                    timezoneMsg,
                    timezoneResponse
                };

                var parseCheck = DateTime.TryParse($"{year}-{month}-{day} {time} {timezone}", out var date);
                if (!parseCheck)
                {
                    await Context.ReplyAsync("Couldn't parse input into a timestamp\n" +
                                             $"Input: {year}-{month}-{day} {time} {timezone}", Color.Red.RawValue);
                    await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
                    return;
                }

                if (date == DateTime.UtcNow.Date)
                    await Context.ReplyAsync("Can't schedule an event on the same day.", Color.Red.RawValue);

                var check = await db.EventSchedules.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.Time == date);
                if (check != null)
                {
                    await Context.ReplyAsync("There is already an event scheduled at that time.", Color.Red.RawValue);
                    return;
                }

                var id = await db.EventSchedules.CountAsync(x => x.GuildId == Context.Guild.Id) + 1;

                var embed = new EmbedBuilder().CreateDefault(Context.Guild.Id)
                    .WithTitle(name)
                    .WithTimestamp(new DateTimeOffset(date));


                embed.AddField("Host", Context.User.Mention ?? ":KuuThinking:");
                await Context.ReplyAsync(embed, "Does this look correct? (y/n)");
                try
                {
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                    if (response.Content.ToLower() == "n")
                    {
                        await Context.ReplyAsync(
                            "Cancelling event scheduling. Restart with new designated name/time.\n" +
                            $"Date input: {year}-{month}-{day} {time} {timezone}\n" +
                            $"Name: {name}");
                        await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
                        return;
                    }

                    if (response.Content.ToLower() == "y")
                    {
                        await _service.TryAddEventAsync(name, Context.User as IGuildUser, date, db);
                        var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                        await Context.ReplyAsync(
                            $"Scheduled {name} for {time.Humanize()} \nUse `event desc {id} <description>` to add a description to your event\nUse `event image {id} <imageUrl>` to add a image to your event!");
                        if (cfg.DesignChannel.HasValue)
                            await Context.Guild.GetTextChannel(cfg.DesignChannel.Value).ReplyAsync(embed,
                                $"New event added\nClaim to make a banner for this with `!event claim {id}`");
                        await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [Command("preview", RunMode = RunMode.Async)]
        [Summary("Previews a event given the ID")]
        [WhiteListedOverAll]
        public async Task PreviewEvent(int id)
        {
            using (var db = new DbService())
            {
                var eventInfo = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventInfo == null)
                {
                    await Context.ReplyAsync("Couldn't find a event with given ID.");
                    return;
                }

                var embed = new EmbedBuilder().CreateDefault(eventInfo.Description, Context.Guild.Id)
                    .WithImageUrl(eventInfo.ImageUrl)
                    .WithTitle(eventInfo.Name)
                    .WithTimestamp(new DateTimeOffset(eventInfo.Time));
                await Context.ReplyAsync(embed,
                    "Preview. Colour does not represent the colour displayed in schedule channel.");
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
                    await Context.ReplyAsync("You do not own or have the permission to remove events!",
                        Color.Red.RawValue);
                    return;
                }

                await _service.TryRemoveEventAsync(id, Context.Guild, db);
                await Context.ReplyAsync($"Removed {eventData.Name} from the event schedule.", Color.Green.RawValue);
            }
        }

        [Command("claim", RunMode = RunMode.Async)]
        [Summary("Claims a event to make a image for it")]
        [WhiteListedDesigner]
        public async Task ClaimEventAsync(int id)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var eventData =
                    await db.EventSchedules.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Id == id);
                if (eventData == null) return;

                if (eventData.DesignerClaim.HasValue)
                {
                    await Context.ReplyAsync($"{Context.User.Mention}, this event is already claimed",
                        Color.Red.RawValue);
                    return;
                }

                eventData.DesignerClaim = Context.User.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"{Context.User.Mention} has claimed to make a banner for {eventData.Name}",
                    Color.Green.RawValue);
            }
        }

        [Command("Description", RunMode = RunMode.Async)]
        [Alias("desc")]
        [Summary("Sets a description to the event")]
        [WhiteListedEventOrg]
        public async Task SetEventDescriptionAsync(int id, [Remainder] string content)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventData == null) return;

                eventData.Description = content;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Updated description for {eventData.Name}", Color.Green.RawValue);
            }
        }

        [Command("Image", RunMode = RunMode.Async)]
        [Alias("img")]
        [Summary("Sets a image to the event")]
        [WhiteListedOverAll]
        public async Task SetEventImageUrlAsync(int id, string url)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventData == null) return;

                if (!eventData.DesignerClaim.HasValue) eventData.DesignerClaim = Context.User.Id;

                eventData.ImageUrl = url;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Updated event image for {eventData.Name}", Color.Green.RawValue);
            }
        }
    }
}