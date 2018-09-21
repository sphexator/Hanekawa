using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
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
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddEventAsync(DateTime time,[Remainder] string name)
        {
            using (var db = new DbService())
            {
                if (time.Date == DateTime.UtcNow.Date)
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Can't schedule an event on the same day.", Color.Red.RawValue)
                            .Build());
                var check = await db.EventSchedules.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.Time == time);
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
                    Timestamp = new DateTimeOffset(time),
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
                        await _service.TryAddEventAsync(db, name, Context.User as IGuildUser, time);
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply(
                                    $"Scheduled {name} for {time.Humanize()} \nUse `event desc {id} <description>` to add a description to your event\nUse `event image {id} <imageUrl>` to add a image to your event!")
                                .Build());
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

        [Command("Description", RunMode = RunMode.Async)]
        [Alias("desc")]
        [Summary("Sets a description to the event")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
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
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SetEventImageUrlAsync(int id, string url)
        {
            using (var db = new DbService())
            {
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if (eventData == null) return;
                eventData.ImageUrl = url;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Updated event image for {eventData.Name}", Color.Green.RawValue)
                        .Build());
            }
        }
    }
}