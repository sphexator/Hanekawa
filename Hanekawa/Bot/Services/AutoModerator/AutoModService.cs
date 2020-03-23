using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Bot.Services.Administration.Mute;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.AutoModerator
{
    public class AutoModService
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;
        private readonly LogService _logService;
        private readonly MuteService _muteService;

        public AutoModService(DiscordBot client, LogService logService, MuteService muteService,
            InternalLogService log)
        {
            _client = client;
            _logService = logService;
            _muteService = muteService;
            _log = log;

            _client.MessageReceived += MessageLength;
            _client.MessageReceived += InviteFilter;
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
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                        if (!cfg.FilterInvites) return;
                        await e.Message.TryDeleteMessagesAsync();
                        await _muteService.Mute(user, db);
                        await _logService.Mute(user, user.Guild.CurrentMember, $"Invite link - {invite.Truncate(80)}",
                            db);
                    }

                    _log.LogAction(LogLevel.Information, $"(Automod) Deleted message from {user.Id} in {user.Guild.Id}. reason: Invite link ({invite})");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Automod) Error in {channel.Guild.Id} for Invite link - {e.Message}");
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
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                        if (!cfg.FilterMsgLength.HasValue) return;
                        if (message.Content.Length < cfg.FilterMsgLength.Value) return;
                        await message.TryDeleteMessagesAsync();
                    }

                    _log.LogAction(LogLevel.Information, $"(Automod) Deleted message from {user.Id} in {user.Guild.Id}. reason: Message length ({message.Content.Length})");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Automod) Error in {channel.Guild.Id} for Message Length - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}