using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Services.Level
{
    public class LevelService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly Random _random;
        private readonly Cooldown _cooldown;
        private readonly ExperienceHandler _experience;
        private readonly LogService _log;

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverCategoryReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverChannelReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public LevelService(DiscordSocketClient client, Random random, Cooldown cooldown, ExperienceHandler experience, LogService log)
        {
            _client = client;
            _random = random;
            _cooldown = cooldown;
            _experience = experience;
            _log = log;

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.LevelExpReductions)
                {
                    if (x.Category)
                    {
                        var cateogries = _serverCategoryReduction.GetOrAdd(x.GuildId, new List<ulong>());
                        cateogries.Add(x.ChannelId);
                        _serverCategoryReduction.AddOrUpdate(x.GuildId, new List<ulong>(),
                            (arg1, list) => cateogries);
                    }

                    if (x.Channel)
                    {
                        var channel = _serverChannelReduction.GetOrAdd(x.GuildId, new List<ulong>());
                        channel.Add(x.ChannelId);
                        _serverCategoryReduction.AddOrUpdate(x.GuildId, new List<ulong>(),
                            (arg1, list) => channel);
                    }
                }
            }
        }

        private Task ServerMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!ValidateUser(message)) return;
                    var channel = message.Channel as ITextChannel;
                    var user = message.Author as SocketGuildUser;
                    if (!_cooldown.ServerCooldown(user)) return;

                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateUserData(user);

                        userdata.LastMessage = DateTime.UtcNow;
                        if (!userdata.FirstMessage.HasValue ||
                            userdata.FirstMessage.Value - TimeSpan.FromDays(999) > userdata.FirstMessage.Value)
                            userdata.FirstMessage = DateTime.UtcNow;

                        await _experience.AddExp(db, user, GetMessageExp(IsReducedExp(channel)), GetMessageCredit());
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e.ToString(), "Level service");
                }
            });
            return Task.CompletedTask;
        }

        private Task GlobalMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!ValidateUser(message)) return;
                    var channel = message.Channel as ITextChannel;
                    var user = message.Author as SocketGuildUser;

                    if (!_cooldown.GlobalCooldown(user)) return;

                    await _experience.AddGlobalExp(user, GetMessageExp(IsReducedExp(channel)), GetMessageCredit());
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e.ToString(), "Level service");
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                if (!(user is SocketGuildUser guser)) return;
                if (user.IsBot) return;
                using (var db = new DbService())
                {
                    try
                    {
                        var userdata = await db.GetOrCreateUserData(guser);
                        await _experience.AddVoiceExp(db, guser, GetVoiceExp(userdata.VoiceExpTime),
                            GetVoiceCredit(userdata.VoiceExpTime), userdata, oldState, newState);
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e.ToString(), "Level Voice");
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static bool ValidateUser(SocketMessage message)
        {
            if (message.Author.IsBot) return false;
            if (!(message is SocketUserMessage msg)) return false;
            if (msg.Source != MessageSource.User) return false;
            if (!(msg.Channel is ITextChannel)) return false;
            return msg.Author is SocketGuildUser;
        }

        private int GetMessageExp(bool reduced = false)
        {
            var xp = _random.Next(10, 20);
            return reduced ? Convert.ToInt32(xp / 10) : Convert.ToInt32(xp);
        }

        private int GetMessageCredit() => _random.Next(1, 3);

        private static int GetVoiceExp(DateTime vcTimer)
        {
            var diff = DateTime.UtcNow - vcTimer;
            return VoiceCalculate(diff.Hours, diff.Minutes) * 2;
        }

        private static int GetVoiceCredit(DateTime vcTimer)
        {
            var diff = DateTime.UtcNow - vcTimer;
            return VoiceCalculate(diff.Hours, diff.Minutes);
        }

        private static int VoiceCalculate(int hours, int minutes) => hours * 60 * minutes;

        private bool IsReducedExp(ITextChannel channel)
        {
            var isChannel = _serverChannelReduction.TryGetValue(channel.GuildId, out var channels);
            var isCategory = _serverCategoryReduction.TryGetValue(channel.GuildId, out var category);
            if (isCategory)
            {
                if (channel.CategoryId.HasValue)
                {
                    if (category.Contains(channel.CategoryId.Value)) return true;
                }
            }
            if (isChannel)
            {
                if (channels.Contains(channel.Id)) return true;
            }

            return false;
        }

        public async Task<EmbedBuilder> ReducedExpManager(ITextChannel channel, bool remove)
        {
            using (var db = new DbService())
            {
                var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
                if (!remove)
                {
                    if (!channels.Contains(channel.Id)) return await AddReducedExp(db, channel);
                    return new EmbedBuilder().CreateDefault($"{channel.Mention} is already added.", Color.Red.RawValue);
                }

                if (channels.Contains(channel.Id)) return await RemoveReducedExp(db, channel);
                return new EmbedBuilder().CreateDefault($"Couldn't find {channel.Mention}", Color.Red.RawValue);
            }
        }

        public async Task<EmbedBuilder> ReducedExpManager(ICategoryChannel category, bool remove)
        {
            using (var db = new DbService())
            {
                if (!remove) return await AddReducedExp(db, category);

                var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
                if (channels.Contains(category.Id)) return await RemoveReducedExp(db, category);
                return new EmbedBuilder().CreateDefault($"Couldn't find {category.Name}", Color.Red.RawValue);
            }
        }

        public async Task<List<string>> ReducedExpList(IGuild guild)
        {
            using (var db = new DbService())
            {
                var channels = _serverChannelReduction.GetOrAdd(guild.Id, new List<ulong>());
                var categories = _serverCategoryReduction.GetOrAdd(guild.Id, new List<ulong>());
                var result = new List<string>();
                if (channels.Count == 0 && categories.Count == 0)
                {
                    result.Add("No channels");
                    return result;
                }

                if (channels.Count > 0)
                    foreach (var x in channels)
                        result.Add($"Channel: {(await guild.GetTextChannelAsync(x)).Name}");

                if (categories.Count <= 0) return result;
                {
                    foreach (var x in categories)
                        result.Add($"Category: {(await guild.GetCategoriesAsync()).First(y => y.Id == x).Name}\n");
                }

                return result;
            }
        }

        private async Task<EmbedBuilder> AddReducedExp(DbService db, ITextChannel channel)
        {
            var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
            channels.Add(channel.Id);
            var data = new LevelExpReduction
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                Channel = true,
                Category = false
            };
            await db.LevelExpReductions.AddAsync(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Added {channel.Name} to reduced exp list", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> AddReducedExp(DbService db, ICategoryChannel category)
        {
            var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
            channels.Add(category.Id);
            var data = new LevelExpReduction
            {
                GuildId = category.GuildId,
                ChannelId = category.Id,
                Channel = false,
                Category = true
            };
            await db.LevelExpReductions.AddAsync(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Added {category.Name} to reduced exp list", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveReducedExp(DbService db, ITextChannel channel)
        {
            var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
            channels.Remove(channel.Id);
            var data = await db.LevelExpReductions.FindAsync(channel.GuildId, channel.Id);
            db.LevelExpReductions.Remove(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Removed {channel.Name} from reduced exp list",
                Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveReducedExp(DbService db, ICategoryChannel category)
        {
            var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
            channels.Remove(category.Id);
            var data = await db.LevelExpReductions.FindAsync(category.GuildId, category.Id);
            db.LevelExpReductions.Remove(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Removed {category.Name} from reduced exp list",
                Color.Green.RawValue);
        }
    }
}
