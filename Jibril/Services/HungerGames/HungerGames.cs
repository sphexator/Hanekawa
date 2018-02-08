using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.HungerGames
{
    public class HungerGames : IJob
    {
        private readonly DiscordSocketClient _client;
        private readonly List<ulong> _eventStartMsg;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
        }


        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public static async Task EventStart()
        {

        }
    }
}
