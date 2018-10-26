using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hanekawa.Services.Administration
{
    public class PlayStatusService
    {
        private DiscordSocketClient _client;
        private Timer _updateStatus;
        private int _memberCount;
        public PlayStatusService(DiscordSocketClient client)
        {
            _client = client;

            _client.Ready += _client_Ready;
            _memberCount = 0;
        }

        private Task _client_Ready()
        {
            _updateStatus = new Timer(async _ =>
            {
                var users = 0;
                foreach (var x in _client.Guilds)
                {
                    users += x.MemberCount;
                }
                if (users == _memberCount) return;
                _memberCount = users;
                await _client.SetGameAsync($"Serving {users} users", null, ActivityType.Playing);
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }
    }
}
