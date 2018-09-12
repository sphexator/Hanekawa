using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Hanekawa.Services.Events {
    public class EventService : IJob {
        private readonly DiscordSocketClient _client;
        public EventService (DiscordSocketClient client) {
            _client = client;
        }

        public Task Execute (IJobExecutionContext context) => EventScheduler ();

        public async Task<bool> TryAddEventAsync (string name, IGuildUser user, DateTime time) {
            using (var db = new DbService ()) {
                var check = await db.EventSchedules.FindAsync ((await db.EventSchedules.CountAsync (x => x.GuildId == user.Guild.Id) + 1) user.Guild.Id);
                if (check != null) return false;
                var data = new EventSchedule {
                    Id = await db.EventSchedules.CountAsync (x => x.GuildId == user.Guild.Id) + 1,
                    GuildId = user.Guild.Id,
                    Host = user.Id,
                    Time = time
                };
                await db.EventSchedules.AddAsync (data);
                await db.SaveChangesAsync ();
                return true;
            }
        }

        public async Task<bool> TryRemoveEventAsync (int id, IGuild guild) {
            using (var db = new DbService ()) {
                var eventData = await db.EventSchedules.FirstOrDefaultAsync (x => x.GuildId == guild.Id && x.Id == id);
                if (eventData == null) return false;
                db.EventSchedules.Remove (eventData);
                await db.SaveChangesAsync ();
                return true;
            }
        }

        private async Task EventScheduler () {
            using (var db = new DbService ()) {
                var guilds = await db.GuildConfigs.Where (x => x.Premium && x.AutomaticEventSchedule).ToListAsync ();
                foreach (var x in guilds) {
                    if (!x.EventChannel.HasValue) return;
                    
                    var guild = _client.GetGuild (x.GuildId);
                    var channel = guild.GetTextChannel (x.EventChannel.Value);
                    await ChannelCleanup(channel);

                    var eventEmbeds = new List<EmbedBuilder> ();
                    var events = await db.EventSchedules.Where (y => y.GuildId == x.GuildId && y.Time >= DateTime.UtcNow).OrderByDescending(z => z.Time).ToListAsync ();
                    foreach (var e in events) {
                        var embed = new EmbedBuilder {
                            Description = e.Description,
                            ImageUrl = e.ImageUrl,
                            Title = e.Name,
                            Timestamp = new DateTimeOffset (e.Time),
                            Color = GetEventColor (e.Time)
                        };
                        embed.AddField ("Host", guild.GetUser (e.Host).Mention ?? ":KuuThinking:");
                        await channel.SendMessageAsync(null, false, embed.Build());
                        await Task.Delay(2000);
                    }
                }
            }
        }

        private Color GetEventColor (DateTime date) {
            Color result = Color.Purple;
            switch (date.DayOfWeek) {
                case DayOfWeek.Monday:
                    result = Color.Gold;
                    break;
                case DayOfWeek.Tuesday:
                    result = Color.Magenta;
                    break;
                case DayOfWeek.Wednesday:
                    result = Color.Green;
                    break;
                case DayOfWeek.Thursday:
                    result = Color.Orange;
                    break;
                case DayOfWeek.Friday:
                    result = Color.Blue;
                    break;
                case DayOfWeek.Saturday:
                    result = Color.Purple;
                    break;
                case DayOfWeek.Sunday:
                    result = Color.Red;
                    break;
            }
            return result;
        }

        private async Task ChannelCleanup(ITextChannel channel){
            var msgs = await channel.GetMessagesAsync().FlattenAsync();
            await channel.DeleteMessagesAsync(msgs);
        }
    }
}