using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Hosting;
using Hanekawa.Bot.Service.Administration;
using Hanekawa.Bot.Service.Administration.Mute;
using Hanekawa.Bot.Service.Board;
using Hanekawa.Bot.Service.Boost;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Drop;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.Game;
using Hanekawa.Bot.Service.Logs;
using Hanekawa.Database;
using Hanekawa.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Hanekawa.Bot.Service
{
    public class EventHandler : DiscordClientService
    {
        private readonly BlacklistService _blacklist;
        private readonly BoardService _boardService;
        private readonly BoostService _boostService;
        private readonly CacheService _cache;
        private readonly DropService _dropService;
        private readonly ExpService _experience;
        private readonly HungerGameService _hungerGame;
        private readonly LogService _logService;
        private readonly MuteService _mute;
        private readonly IServiceProvider _provider;
        private readonly VoiceRoleService _voiceRole;

        public EventHandler(ILogger<EventHandler> logger, DiscordClientBase client, ExpService experience, CacheService cache,
            LogService logService, BlacklistService blacklist, MuteService mute, BoardService boardService,
            BoostService boostService, DropService dropService, HungerGameService hungerGame, IServiceProvider provider, 
            VoiceRoleService voiceRole) : base(logger, client)
        {
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
            _voiceRole = voiceRole;
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            foreach (var x in db.LevelConfigs)
            {
                _cache.AdjustExpMultiplier(ExpSource.Text, x.GuildId, x.TextExpMultiplier);
                _cache.AdjustExpMultiplier(ExpSource.Voice, x.GuildId, x.VoiceExpMultiplier);
            }

            foreach (var x in db.LevelExpReductions)
                _cache.TryAddExpChannelReduction(x.ChannelId);

            foreach (var x in db.GuildConfigs)
            {
                _cache.AddOrUpdatePrefix(x.GuildId, new StringPrefix(x.Prefix));
                _cache.AddOrUpdateColor(x.GuildId, new Color(x.EmbedColor));
            }
        }

        protected override async ValueTask OnInviteDeleted(InviteDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _logService.InviteDeletedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnInviteCreated(InviteCreatedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _logService.InviteCreatedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _logService.MessageDeletedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            await _logService.MessagesDeletedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _logService.MessageUpdatedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            await _mute.MuteCheck(e).ConfigureAwait(false);
            await _logService.JoinLogAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            await _logService.LeaveLogAsync(e).ConfigureAwait(false);
            await _hungerGame.UserLeftAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            await _logService.MemberUpdatedAsync(e).ConfigureAwait(false);
            await _boostService.BoostCheckAsync(e).ConfigureAwait(false);
            await _hungerGame.UpdateUserAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            await _blacklist.BlackListAsync(e).ConfigureAwait(false);
        }

        protected override ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            _cache.Dispose(e.GuildId);
            return ValueTask.CompletedTask;
        }

        protected override async ValueTask OnBanDeleted(BanDeletedEventArgs e)
        {
            await _logService.UnbanAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnBanCreated(BanCreatedEventArgs e)
        {
            await _logService.BanAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            if (e.Member.IsBot) return;
            await _experience.VoiceExperienceAsync(e).ConfigureAwait(false);
            await _logService.VoiceLogAsync(e).ConfigureAwait(false);
            await _voiceRole.VoiceStateUpdateAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _boardService.ReactionClearedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            await _boardService.ReactionRemovedAsync(e).ConfigureAwait(false);
            await _hungerGame.ReactionRemovedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member.IsBot) return;
            await _boardService.ReactionReceivedAsync(e).ConfigureAwait(false);
            await _dropService.ReactionReceived(e).ConfigureAwait(false);
            await _hungerGame.ReactionReceivedAsync(e).ConfigureAwait(false);
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member.IsBot) return;
            var cdCheck = _cache.TryGetCooldown(e.GuildId.Value, e.Member.Id, CooldownType.ServerMessage);
            if (!cdCheck) _cache.AddCooldown(e.GuildId.Value, e.Member.Id, CooldownType.ServerMessage);
            if (!cdCheck) await _experience.ServerExperienceAsync(e);
            if (!cdCheck) await _dropService.MessageReceived(e);
            await _experience.GlobalExperienceAsync(e);
        }

        protected override ValueTask OnChannelDeleted(ChannelDeletedEventArgs e)
        {
            return base.OnChannelDeleted(e);
        }

        protected override ValueTask OnRoleDeleted(RoleDeletedEventArgs e)
        {
            return base.OnRoleDeleted(e);
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}