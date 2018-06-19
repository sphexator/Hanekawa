using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly bool _disableBanner;

        public WelcomeService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _client = discord;
            _provider = provider;

            _client.UserJoined += Welcomer;

            _disableBanner = false;
        }

        private Task Welcomer(SocketGuildUser user)
        {
            if (_disableBanner) return Task.CompletedTask;
            var _ = Task.Run(async () =>
            {
                if (user.IsBot != true)
                {

                }
            });
            return Task.CompletedTask;
        }
    }
}