using System;
using System.Threading.Tasks;
using Discord.WebSocket;
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
        private readonly DiscordSocketClient _client;
        private readonly LogService _logService;
        private readonly InternalLogService _log;
        private readonly MuteService _muteService;

        public AutoModService(DiscordSocketClient client, LogService logService, MuteService muteService, InternalLogService log)
        {
            _client = client;
            _logService = logService;
            _muteService = muteService;
            _log = log;

            _client.MessageReceived += MessageLength;
            _client.MessageReceived += InviteFilter;
        }

        private Task InviteFilter(SocketMessage message)
        {
            _ = Task.Run(async () =>
            {
                if (!(message.Channel is SocketTextChannel channel)) return;
                if (!(message.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                if (user.GuildPermissions.ManageGuild) return;
                if(!message.Content.IsDiscordInvite(out var invite)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                        if (!cfg.FilterInvites) return;
                        await message.TryDeleteMessagesAsync();
                        await _muteService.Mute(user, db);
                        await _logService.Mute(user, user.Guild.CurrentUser, $"Invite link - {invite.Truncate(80)}", db);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Automod) Error in {channel.Guild.Id} for Invite link - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageLength(SocketMessage message)
        {
            _ = Task.Run(async () =>
            {
                if (!(message.Channel is SocketTextChannel channel)) return;
                if (!(message.Author is SocketGuildUser user)) return;
                if (user.IsBot) return;
                if (user.GuildPermissions.ManageMessages) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateAdminConfigAsync(user.Guild);
                        if (!cfg.FilterMsgLength.HasValue) return;
                        if (message.Content.Length < cfg.FilterMsgLength.Value) return;
                        await message.TryDeleteMessagesAsync();
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, $"(Automod) Error in {channel.Guild.Id} for Message Length - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}
