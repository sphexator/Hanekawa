using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _discord = discord;
            _provider = provider;

            _discord.MessageReceived += GiveExp;
        }

        public async Task GiveExp(SocketMessage msg)
        {

        }

    }
}
