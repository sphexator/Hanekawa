using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Bot.Services.Experience
{
    public class LevelService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly Random _random;
        private readonly InternalLogService _logService;
        private readonly ExpService _exp;
        
        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverCategoryReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();
        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverChannelReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public LevelService(DiscordSocketClient client, Random random, InternalLogService logService, DbService db, 
            ExpService exp)
        {
            _client = client;
            _random = random;
            _logService = logService;
            _db = db;
            _exp = exp;

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            
            foreach (var x in _db.LevelExpReductions)
            {
                if (x.Category)
                {
                    var categories = _serverCategoryReduction.GetOrAdd(x.GuildId, new List<ulong>());
                    categories.Add(x.ChannelId);
                    _serverCategoryReduction.AddOrUpdate(x.GuildId, new List<ulong>(),
                        (arg1, list) => categories);
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

        private Task GlobalMessageExpAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!(msg.Channel is SocketTextChannel channel)) return;
                if (user.IsBot) return;
                if (_exp.OnGlobalCooldown(user)) return;

                var userdata = await _db.GetOrCreateGlobalUserData(user);
                await _exp.AddExp(user, userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3));
            });
            return Task.CompletedTask;
        }

        private Task ServerMessageExpAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!(msg.Channel is SocketTextChannel channel)) return;
                if (user.IsBot) return;
                if (_exp.OnServerCooldown(user)) return;

                var userdata = await _db.GetOrCreateUserData(user);
                userdata.LastMessage = DateTime.UtcNow;
                if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                await _exp.AddExp(user, userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3));
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(SocketUser usr, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if (!(usr is SocketGuildUser user)) return;
            });
            return Task.CompletedTask;
        }
        
        private int GetMessageExp(bool reduced = false)
        {
            var xp = _random.Next(10, 20);
            return reduced ? Convert.ToInt32(xp / 10) : xp;
        }
        
        private bool IsReducedExp(SocketTextChannel channel)
        {
            var isChannel = _serverChannelReduction.TryGetValue(channel.Guild.Id, out var channels);
            var isCategory = _serverCategoryReduction.TryGetValue(channel.Guild.Id, out var category);
            if (!isCategory) return isChannel && channels.Contains(channel.Id);
            if (!channel.CategoryId.HasValue) return isChannel && channels.Contains(channel.Id);
            if (category.Contains(channel.CategoryId.Value))
                return true;
            return isChannel && channels.Contains(channel.Id);
        }
    }
}