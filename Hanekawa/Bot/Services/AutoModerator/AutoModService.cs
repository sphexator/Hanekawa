using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.Services.Administration.Mute;
using Hanekawa.Bot.Services.Logging;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.AutoModerator
{
    public class AutoModService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly LogService _logService;
        private readonly MuteService _muteService;

        public AutoModService(DiscordSocketClient client, DbService db, LogService logService, MuteService muteService)
        {
            _client = client;
            _db = db;
            _logService = logService;
            _muteService = muteService;

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
                var cfg = await _db.GetOrCreateAdminConfigAsync(user.Guild);
                if (!cfg.FilterInvites) return;
                await message.TryDeleteMessagesAsync();
                await _muteService.Mute(user);
                await _logService.Mute(user, user.Guild.CurrentUser, "Invite link");
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
                var cfg = await _db.GetOrCreateAdminConfigAsync(user.Guild);
                if (!cfg.FilterMsgLength.HasValue) return;
                if (message.Content.Length < cfg.FilterMsgLength.Value) return;
                await message.TryDeleteMessagesAsync();
            });
            return Task.CompletedTask;
        }
    }
}
