using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Humanizer;

namespace Hanekawa.Services.Level
{
    public class ExpEventHandler : IHanaService
    {
        private readonly DiscordSocketClient _client;
        private readonly ExperienceHandler _handler;

        private readonly ConcurrentDictionary<ulong, Timer> _expEvent
            = new ConcurrentDictionary<ulong, Timer>();

        public ExpEventHandler(DiscordSocketClient client, ExperienceHandler handler)
        {
            _client = client;
            _handler = handler;
        }

        public async Task InitializeAsync()
        {
            using (var db = new DbService())
            {
                foreach (var x in db.LevelConfigs)
                {
                    var levelExpEvent = await db.LevelExpEvents.FindAsync(x.GuildId);
                    if (levelExpEvent == null)
                    {
                        _handler.AdjustMultiplier(x.GuildId, x.ExpMultiplier);
                    }
                    else if (levelExpEvent.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow)
                    {
                        _handler.AdjustMultiplier(x.GuildId, x.ExpMultiplier);
                        var check = db.LevelExpEvents.FirstOrDefault(y => y.GuildId == x.GuildId);
                        if (check == null) continue;
                        db.LevelExpEvents.Remove(check);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var after = levelExpEvent.Time - DateTime.UtcNow;
                        CreateEvent(levelExpEvent.GuildId, levelExpEvent.Multiplier, x.ExpMultiplier,
                            levelExpEvent.MessageId,
                            levelExpEvent.ChannelId, after);
                    }
                }
            }
        }

        public async Task StartAsync(DbService db, IGuild guild, int multiplier, TimeSpan after) =>
            await ExecuteAsync(db, guild, multiplier, after, false, null);

        public async Task StartAsync(DbService db, IGuild guild, int multiplier, TimeSpan after, bool announce) =>
            await ExecuteAsync(db, guild, multiplier, after, announce, null);

        public async Task StartAsync(DbService db, IGuild guild, int multiplier, TimeSpan after, bool announce,
            ITextChannel fallbackChannel) =>
            await ExecuteAsync(db, guild, multiplier, after, announce, fallbackChannel);

        public void CreateEvent(ulong guildId, int multiplier, int defaultMulti, ulong? messageId,
            ulong? channelId, TimeSpan after)
        {
            _handler.AdjustMultiplier(guildId, multiplier);
            var toAdd = new Timer(async _ =>
            {
                using (var db = new DbService())
                {
                    try
                    {
                        _handler.AdjustMultiplier(guildId, defaultMulti);
                        if (messageId.HasValue && channelId.HasValue)
                            if (await _client.GetGuild(guildId).GetTextChannel(channelId.Value)
                                .GetMessageAsync(messageId.Value) is IUserMessage msg)
                            {
                                var upd = msg.Embeds.First().ToEmbedBuilder();
                                upd.Color = Color.Red;
                                upd.Footer = new EmbedFooterBuilder {Text = "Ended"};
                                await msg.ModifyAsync(x => x.Embed = upd.Build());
                            }

                        RemoveFromDatabase(db, guildId);
                        _expEvent.Remove(guildId, out var _);
                    }
                    catch
                    {
                        _handler.AdjustMultiplier(guildId, defaultMulti);
                        RemoveFromDatabase(db, guildId);
                    }
                }
            }, null, after, Timeout.InfiniteTimeSpan);
            _expEvent.AddOrUpdate(guildId, key => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private async Task ExecuteAsync(DbService db, IGuild guild, int multiplier, TimeSpan after, bool announce,
            ITextChannel fallbackChannel)
        {
            IUserMessage message = null;
            var cfg = await db.GetOrCreateLevelConfigAsync(guild as SocketGuild);
            if (announce) message = await AnnounceExpEventAsync(db, await db.ChannelConfigs.FindAsync(guild), guild, multiplier, after, fallbackChannel);
            CreateEvent(guild.Id, multiplier, cfg.ExpMultiplier, message?.Id, message?.Channel.Id, after);
            await EventAddOrUpdateDatabaseAsync(db, guild.Id, multiplier, message?.Id, message?.Channel.Id, after);
        }

        private async Task EventAddOrUpdateDatabaseAsync(DbService db, ulong guildId, int multiplier,
            ulong? message, ulong? channel, TimeSpan after)
        {
            var check = await db.LevelExpEvents.FindAsync(guildId);
            if (check == null)
            {
                var data = new LevelExpEvent
                {
                    GuildId = guildId,
                    MessageId = message,
                    ChannelId = channel,
                    Multiplier = multiplier,
                    Time = DateTime.UtcNow + after
                };
                await db.LevelExpEvents.AddAsync(data);
                await db.SaveChangesAsync();
            }
            else
            {
                check.ChannelId = channel;
                check.Time = DateTime.UtcNow + after;
                check.Multiplier = multiplier;
                check.MessageId = message;
                await db.SaveChangesAsync();
            }
        }

        private void RemoveFromDatabase(DbService db, ulong guildId)
        {
            var check = db.LevelExpEvents.FirstOrDefault(x => x.GuildId == guildId);
            if (check == null) return;
            db.LevelExpEvents.Remove(check);
            db.SaveChanges();
        }

        private async Task<IUserMessage> AnnounceExpEventAsync(DbService db, ChannelConfig cfg, IGuild guild,
            int multiplier, TimeSpan after, IMessageChannel fallbackChannel)
        {
            var check = await db.LevelExpEvents.FindAsync(guild.Id);
            if (check?.ChannelId == null || !check.MessageId.HasValue)
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            try
            {
                var msg = await _client.GetGuild(guild.Id).GetTextChannel(check.ChannelId.Value)
                    .GetMessageAsync(check.MessageId.Value);
                if (msg is null) return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);

                await msg.DeleteAsync();
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            }
            catch
            {
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            }
        }

        private static async Task<IUserMessage> PostAnnouncementAsync(IGuild guild, ChannelConfig cfg, int multiplier,
            TimeSpan after, IMessageChannel fallbackChannel)
        {
            if (cfg.EventChannel.HasValue)
            {
                var channel = await guild.GetTextChannelAsync(cfg.EventChannel.Value);
                var embed = new EmbedBuilder().CreateDefault($"A {multiplier}x exp event has started!\n" +
                                                             $"Duration: {after.Humanize()} ( {after} )", cfg.GuildId);
                embed.Title = $"{multiplier}x Experience Event";
                embed.Timestamp = DateTimeOffset.UtcNow + after;
                embed.Footer = new EmbedFooterBuilder {Text = "Ends:"};
                var msg = await channel.ReplyAsync(embed);
                return msg;
            }

            await fallbackChannel.SendMessageAsync(null, false,
                new EmbedBuilder().CreateDefault("No event channel has been setup.",
                    Color.Red.RawValue).Build());
            return null;
        }
    }
}