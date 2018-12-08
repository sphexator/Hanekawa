using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Administration;
using Hanekawa.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Hanekawa.Services.Events
{
    public class EventService : IJob, IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public EventService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;
            Console.WriteLine("Event service loaded");
        }

        public Task Execute(IJobExecutionContext context)
        {
            return EventSchedulerAsync();
        }

        public async Task Execute()
        {
            await EventSchedulerAsync();
        }

        public async Task<bool> TryAddEventAsync(string name, IGuildUser user, DateTime time)
        {
            var check = await _db.EventSchedules.FindAsync(
                await _db.EventSchedules.CountAsync(x => x.GuildId == user.Guild.Id) + 1,
                user.Guild.Id);
            if (check != null) return false;
            var data = new EventSchedule
            {
                Id = await _db.EventSchedules.CountAsync(x => x.GuildId == user.Guild.Id) + 1,
                GuildId = user.Guild.Id,
                Host = user.Id,
                Time = time,
                Name = name
            };
            await _db.EventSchedules.AddAsync(data);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TryRemoveEventAsync(int id, IGuild guild)
        {
            var eventData = await _db.EventSchedules.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.Id == id);
            if (eventData == null) return false;
            _db.EventSchedules.Remove(eventData);
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task EventSchedulerAsync()
        {
            using (var db = new DbService())
            {
                var guilds = await db.GuildConfigs.Where(x => x.Premium && x.AutomaticEventSchedule).ToListAsync();
                foreach (var x in guilds)
                {
                    if (!x.EventSchedulerChannel.HasValue) continue;

                    var guild = _client.GetGuild(x.GuildId);
                    if (guild == null) continue;
                    var channel = guild.GetTextChannel(x.EventSchedulerChannel.Value);
                    var events = await db.EventSchedules
                        .Where(y => y.GuildId == x.GuildId && y.Time.Date >= DateTime.UtcNow.Date)
                        .OrderBy(z => z.Time).Take(5).ToListAsync();
                    if (events.All(z => z.Posted)) continue;
                    if (events.Any()) await ChannelCleanup(channel);
                    foreach (var e in events.OrderByDescending(t => t.Time))
                    {
                        var embed = new EmbedBuilder
                        {
                            Description = e.Description,
                            ImageUrl = e.ImageUrl,
                            Title = e.Name,
                            Timestamp = new DateTimeOffset(e.Time),
                            Color = GetEventColor(e.Time),
                            Footer = new EmbedFooterBuilder {Text = "Scheduled:"}
                        };
                        embed.AddField("Host", guild.GetUser(e.Host).Mention ?? ":KuuThinking:");
                        await channel.SendMessageAsync(null, false, embed.Build());
                        await Task.Delay(1000);
                        e.Posted = true;
                    }

                    await db.SaveChangesAsync();
                }
            }
        }

        private static Color GetEventColor(DateTime date)
        {
            var result = Color.Purple;
            switch (date.DayOfWeek)
            {
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
                    result = Color.DarkerGrey;
                    break;
            }

            return result;
        }

        private static async Task ChannelCleanup(ITextChannel channel)
        {
            try
            {
                var msgs = await channel.GetMessagesAsync().FlattenAsync();
                var result = msgs.Where(x => x.Timestamp.Date.AddDays(10) >= DateTime.UtcNow.Date).ToList();
                await channel.DeleteMessagesAsync(result);
            }
            catch
            {
            }
        }
    }
}