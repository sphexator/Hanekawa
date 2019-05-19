using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly Random _random;
        private readonly InternalLogService _log;

        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerCategoryReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();
        public readonly ConcurrentDictionary<ulong, HashSet<ulong>> ServerChannelReduction =
            new ConcurrentDictionary<ulong, HashSet<ulong>>();

        public ExpService(DiscordSocketClient client, Random random, InternalLogService log)
        {
            _client = client;
            _random = random;
            _log = log;

            using (var db = new DbService())
            {
                foreach (var x in db.LevelExpReductions)
                {
                    if (x.Category)
                    {
                        var categories = ServerCategoryReduction.GetOrAdd(x.GuildId, new HashSet<ulong>());
                        categories.Add(x.ChannelId);
                        ServerCategoryReduction.AddOrUpdate(x.GuildId, new HashSet<ulong>(),
                            (arg1, list) => categories);
                    }

                    if (x.Channel)
                    {
                        var channel = ServerChannelReduction.GetOrAdd(x.GuildId, new HashSet<ulong>());
                        channel.Add(x.ChannelId);
                        ServerCategoryReduction.AddOrUpdate(x.GuildId, new HashSet<ulong>(),
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
                try
                {
                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateGlobalUserData(user);
                        await AddExp(userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3), db);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Exp Service) Error in {user.Guild.Id} for Global Exp - {e.Message}");
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
                try
                {
                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateUserData(user);
                        userdata.LastMessage = DateTime.UtcNow;
                        if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                        await AddExp(user, userdata, GetMessageExp(IsReducedExp(channel)), _random.Next(1, 3), db);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Exp Service) Error in {user.Guild.Id} for Server Exp - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(SocketUser usr, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if (!(usr is SocketGuildUser user)) return;
                try
                {
                    // TODO: Voice exp
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Exp Service) Error in {user.Guild.Id} for Voice - {e.Message}");
                }
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
            var isChannel = ServerChannelReduction.TryGetValue(channel.Guild.Id, out var channels);
            var isCategory = ServerCategoryReduction.TryGetValue(channel.Guild.Id, out var category);
            if (!isCategory) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (!channel.CategoryId.HasValue) return isChannel && channels.TryGetValue(channel.Id, out _);
            if (category.TryGetValue(channel.CategoryId.Value, out _))
                return true;
            return isChannel && channels.TryGetValue(channel.Id, out _);
        }
    }
}