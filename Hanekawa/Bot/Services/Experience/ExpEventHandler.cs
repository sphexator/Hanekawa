using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly Random _random;
        private readonly InternalLogService _logService;

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverCategoryReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();
        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverChannelReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public ExpService(DiscordSocketClient client, Random random, InternalLogService logService)
        {
            _client = client;
            _random = random;
            _logService = logService;

            using (var db = new DbService())
            {
                foreach (var x in db.LevelExpReductions)
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

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            _client.UserJoined += GiveRolesBackAsync;
        }

        private Task GlobalMessageExpAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!(msg.Channel is SocketTextChannel channel)) return;
                if (user.IsBot) return;
                if (OnGlobalCooldown(user)) return;
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateGlobalUserData(user);
                    await AddExp(userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3), db);
                }
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
                if (OnServerCooldown(user)) return;
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.LastMessage = DateTime.UtcNow;
                    if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                    await AddExp(user, userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3), db);
                }
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