using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Welcome
{
    public class WelcomeService
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        public WelcomeService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _discord = discord;
            _provider = provider;
            _discord.UserJoined += Welcomer;
        }

        public async Task Welcomer(SocketGuildUser user)
        {
            await Task.Run(() =>
            {

            });
        }
    }
}
