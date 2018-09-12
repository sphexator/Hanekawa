using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Events {
    [Group ("Event")]
    [RequireContext (ContextType.Guild)]
    public class EventModule : InteractiveBase {
        [Command ("Add")]
        public async Task AddEventAsync (DateTime time, string name) {
            using (var db = new DbService ()) {
                if (time.Date == DateTime.UtcNow.Date) {
                    await ReplyAsync (null, false, new EmbedBuilder ().Reply ("Can't schedule an event on the same day.", Color.Red.RawValue).Build ());
                }
                var check = await db.EventSchedules.FirstOrDefaultAsync (x => x.GuildId == Context.Guild.Id && x.Time == time);
                if (check != null) {
                    await ReplyAsync (null, false, new EmbedBuilder ().Reply ("There is already an event scheduled at that time.", Color.Red.RawValue).Build ());
                    return;
                }
                var id = await db.EventSchedules.CountAsync(x => x.GuildId == Context.Guild.Id + 1);
                var data = new EventSchedule {
                    Id = id,
                    GuildId = Context.Guild.Id,
                    Host = Context.User.Id,
                    Time = time,
                    Name = name
                };
                var embed = new EmbedBuilder {
                    Title = name,
                    Timestamp = new DateTimeOffset (time),
                    Color = Color.Purple
                };
                embed.AddField ("Host", Context.User.Mention ?? ":KuuThinking:");
                await ReplyAsync ("Does this look correct? (y/n)", false, embed.Build ());
                try {
                    var response = await NextMessageAsync (true, true, TimeSpan.FromSeconds (60));
                    if (response.Content.ToLower () == "n") {
                        await ReplyAsync (null, false, new EmbedBuilder ().Reply ("Cancelling event scheduling. Restart with new designated name/time (time is in UTC)").Build ());
                        return;
                    }
                    if (response.Content.ToLower () == "y") {
                        await db.EventSchedules.AddAsync (data);
                        await db.SaveChangesAsync ();
                        await ReplyAsync (null, false, new EmbedBuilder ().Reply ($"Scheduled {name} for {time.Humanize()} \nUse `event desc {id} <description>` to add a description to your event\nUse `event image {id} <imageUrl>` to add a image to your event!").Build ());
                    }
                }
                catch {}
            }
        }

        [Command("Remove")]
        public async Task RemoveEventAsync(int id){
            using(var db = new DbService()){
                var eventData = await db.EventSchedules.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Id == id);
                if(eventData == null) return;
                if(eventData.Host != Context.User.Id || !(Context.User as IGuildUser).GuildPermissions.ManageGuild) {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("You do not own or have the permission to remove events!", Color.Red.RawValue).Build());
                    return;
                    }
                db.EventSchedules.Remove(eventData);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Removed {eventData.Name} from the event schedule.", Color.Green.RawValue).Build());
            }
        }

        [Command("Description")]
        [Alias("desc")]
        public async Task SetEventDescriptionAsync(int id, [Remainder]string content){
            using(var db = new DbService()){
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if(eventData == null) return;
                eventData.Description = content;
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Updated description for {eventData.Name}", Color.Green.RawValue).Build());
            }
        }

        [Command("Image")]
        [Alias("img")]
        public async Task SetEventImageUrlAsync(int id, string url){
            using(var db = new DbService()){
                var eventData = await db.EventSchedules.FindAsync(id, Context.Guild.Id);
                if(eventData == null) return;
                eventData.ImageUrl = url;
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Updated event image for {eventData.Name}", Color.Green.RawValue).Build());
            }
        }
    }
}