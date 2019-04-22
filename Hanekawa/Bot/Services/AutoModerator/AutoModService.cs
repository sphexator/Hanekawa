using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hanekawa.Bot.Services.AutoModerator
{
    public class AutoModService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public AutoModService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

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
                if (message.Content.Length >= cfg.FilterMsgLength.Value)
                {
                    await message.TryDeleteMessagesAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}
