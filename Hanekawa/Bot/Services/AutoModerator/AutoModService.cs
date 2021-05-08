using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Bot.Services.Administration.Mute;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Services.AutoModerator
{
    public class AutoModService
    {
        private readonly Hanekawa _client;
        private readonly NLog.Logger _log;
        private readonly LogService _logService;
        private readonly MuteService _muteService;
        private readonly IServiceProvider _provider;

        public AutoModService(Hanekawa client, LogService logService, MuteService muteService, IServiceProvider provider)
        {
            _client = client;
            _logService = logService;
            _muteService = muteService;
            _log = LogManager.GetCurrentClassLogger();
            _provider = provider;

            //_client.MessageReceived += MessageLength;
            //_client.MessageReceived += InviteFilter;
        }

        private Task InviteFilter(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Message.Channel is CachedTextChannel channel)) return;
                if (!(e.Message.Author is CachedMember user)) return;
                if (user.IsBot) return;
                if (user.Permissions.ManageGuild) return;
                if (!e.Message.Content.IsDiscordInvite(out var invite)) return;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                    if (!cfg.FilterInvites) return;
                    await e.Message.TryDeleteMessagesAsync();
                    await _muteService.Mute(user, db);
                    await _logService.Mute(user, user.Guild.CurrentMember, $"Invite link - {invite.Truncate(80)}",
                        db);

                    _log.Log(LogLevel.Info, $"(Automod) Deleted message from {user.Id.RawValue} in {user.Guild.Id.RawValue}. reason: Invite link ({invite})");
                }
                catch (Exception e)
                {
                    _log.Log(NLog.LogLevel.Error, e,
                        $"(Automod) Error in {channel.Guild.Id.RawValue} for Invite link - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageLength(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!(e.Message.Channel is CachedTextChannel channel)) return;
                if (!(e.Message.Author is CachedMember user)) return;
                if (user.IsBot) return;
                if (user.Permissions.ManageMessages) return;
                var message = e.Message;
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                    if (!cfg.FilterMsgLength.HasValue) return;
                    if (message.Content.Length < cfg.FilterMsgLength.Value) return;
                    await message.TryDeleteMessagesAsync();

                    _log.Log(LogLevel.Info, $"(Automod) Deleted message from {user.Id.RawValue} in {user.Guild.Id.RawValue}. reason: Message length ({message.Content.Length})");
                }
                catch (Exception e)
                {
                    _log.Log(NLog.LogLevel.Error, e,
                        $"(Automod) Error in {channel.Guild.Id.RawValue} for Message Length - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}