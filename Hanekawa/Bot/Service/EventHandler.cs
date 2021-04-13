using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Administration;
using Hanekawa.Bot.Service.Administration.Mute;
using Hanekawa.Bot.Service.Board;
using Hanekawa.Bot.Service.Boost;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Drop;
using Hanekawa.Bot.Service.Game;
using Hanekawa.Bot.Service.Logs;
using Hanekawa.Database;
using Hanekawa.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Hanekawa.Bot.Service
{
    public class EventHandler : INService, IRequired, IJob
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;
        private readonly CacheService _cache;
        private readonly Experience _experience;
        private readonly LogService _logService;
        private readonly BlacklistService _blacklist;
        private readonly MuteService _mute;
        private readonly BoardService _boardService;
        private readonly BoostService _boostService;
        private readonly DropService _dropService;
        private readonly HungerGameService _hungerGame;

        public EventHandler(Hanekawa client, Experience experience, CacheService cache, LogService logService, 
            BlacklistService blacklist, MuteService mute, BoardService boardService, BoostService boostService, 
            DropService dropService, HungerGameService hungerGame, IServiceProvider provider)
        {
            _client = client;
            _experience = experience;
            _cache = cache;
            _logService = logService;
            _blacklist = blacklist;
            _mute = mute;
            _boardService = boardService;
            _boostService = boostService;
            _dropService = dropService;
            _hungerGame = hungerGame;
            _provider = provider;
            
            _client.MessageReceived += MessageReceived;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageDeleted += MessageDeleted;
            _client.MessagesDeleted += MessagesDeleted;

            _client.MemberJoined += MemberJoined;
            _client.MemberLeft += MemberLeft;
            _client.MemberUpdated += MemberUpdated;

            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            _client.ReactionsCleared += ReactionsCleared;
            
            _client.VoiceStateUpdated += VoiceStateUpdated;
            
            _client.BanCreated += BanCreated;
            _client.BanDeleted += BanDeleted;
            
            _client.RoleDeleted += RoleDeleted;

            _client.ChannelDeleted += ChannelDeleted;

            _client.JoinedGuild += JoinedGuild;
            _client.LeftGuild += LeftGuild;
            
            _client.InviteCreated += InviteCreated;
            _client.InviteDeleted += InviteDeleted;
            
            _client.Ready += Ready;
        }

        private Task Ready(object sender, ReadyEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();

                foreach (var x in db.LevelConfigs)
                {
                    var expMult = _cache.ExperienceMultipliers.GetOrAdd(new Snowflake(x.GuildId),
                        new ConcurrentDictionary<ExpSource, double>());
                    expMult.TryAdd(ExpSource.Text, x.TextExpMultiplier);
                    expMult.TryAdd(ExpSource.Voice, x.VoiceExpMultiplier);
                }

                foreach (var x in db.LevelExpReductions)
                {
                    var cache = _cache.ExperienceReduction.GetOrAdd(new Snowflake(x.GuildId), new HashSet<Snowflake>());
                    cache.Add(x.ChannelId);
                }

                foreach (var x in db.GuildConfigs)
                {
                    var prefixes = _cache.GuildPrefix.GetOrAdd(new Snowflake(x.GuildId), new HashSet<IPrefix>());
                    prefixes.Add(new StringPrefix(x.Prefix));
                    _cache.GuildEmbedColors.TryAdd(new Snowflake(x.GuildId), new Color((int)x.EmbedColor));
                }
            });
            return Task.CompletedTask;
        }

        private Task InviteDeleted(object sender, InviteDeletedEventArgs e)
        {
            _ = _logService.InviteDeletedAsync(e);
            return Task.CompletedTask;
        }

        private Task InviteCreated(object sender, InviteCreatedEventArgs e)
        {
            _ = _logService.InviteCreatedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessagesDeleted(object sender, MessagesDeletedEventArgs e)
        {
            _ = _logService.MessagesDeletedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            _ = _logService.MessageDeletedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            _ = _logService.MessageUpdatedAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberJoined(object sender, MemberJoinedEventArgs e)
        {
            _ = _logService.JoinLogAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberLeft(object sender, MemberLeftEventArgs e)
        {
            _ = _logService.LeaveLogAsync(e);
            _ = _hungerGame.UserLeftAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberUpdated(object sender, MemberUpdatedEventArgs e)
        {
            _ = _logService.MemberUpdatedAsync(e);
            _ = _boostService.BoostCheckAsync(e);
            _ = _hungerGame.UpdateUserAsync(e);
            return Task.CompletedTask;
        }

        private Task JoinedGuild(object sender, JoinedGuildEventArgs e)
        {
            _ = _blacklist.BlackListAsync(e);
            return Task.CompletedTask;
        }

        private Task LeftGuild(object sender, LeftGuildEventArgs e)
        {
            _cache.Dispose(e.GuildId);
            return Task.CompletedTask;
        }

        private Task BanDeleted(object sender, BanDeletedEventArgs e)
        {
            _ = _logService.UnbanAsync(e);
            return Task.CompletedTask;
        }

        private Task BanCreated(object sender, BanCreatedEventArgs e)
        {
            _ = _logService.BanAsync(e);
            return Task.CompletedTask;
        }
        
        private Task VoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs e)
        {
            _ = _experience.VoiceExperienceAsync(e);
            _ = _logService.VoiceLogAsync(e);
            return Task.CompletedTask;
        }

        private Task ReactionsCleared(object sender, ReactionsClearedEventArgs e)
        {
            _ = _boardService.ReactionClearedAsync(e);
            return Task.CompletedTask;
        }

        private Task ReactionRemoved(object sender, ReactionRemovedEventArgs e)
        {
            _ = _boardService.ReactionRemovedAsync(e);
            _ = _hungerGame.ReactionRemovedAsync(e);
            return Task.CompletedTask;
        }

        private Task ReactionAdded(object sender, ReactionAddedEventArgs e)
        {
            if (e.Member.IsBot) return Task.CompletedTask;
            _ = _boardService.ReactionReceivedAsync(e);
            _ = _dropService.ReactionReceived(e);
            _ = _hungerGame.ReactionReceivedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return Task.CompletedTask;
            if (e.Member.IsBot) return Task.CompletedTask;
            var guildCache = _cache.Cooldown
                .GetOrAdd(e.GuildId.Value, new ConcurrentDictionary<CooldownType, MemoryCache>());
            var msgCache = guildCache.GetOrAdd(CooldownType.ServerMessage, new MemoryCache(new MemoryCacheOptions()));
            var cdCheck = msgCache.TryGetValue(e.Member.Id.RawValue, out _);
            
            if (!cdCheck) msgCache.Set(e.Member.Id.RawValue, 0, TimeSpan.FromMinutes(1));

            guildCache.AddOrUpdate(CooldownType.ServerMessage, msgCache, (_, _) => msgCache);
            if (!cdCheck) _ = _experience.ServerExperienceAsync(e);
            if (!cdCheck) _ = _dropService.MessageReceived(e);
            _ = _experience.GlobalExperienceAsync(e);
            return Task.CompletedTask;
        }
        
        private Task ChannelDeleted(object sender, ChannelDeletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task RoleDeleted(object sender, RoleDeletedEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
