using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Level.Services;
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
            await Task.Run(() =>
            {
                var user = msg.Author as SocketGuildUser;
                var guild = user.Guild.Id;

                //Check if user is in Db or not

                //Getting list of userdata
                //var userData = 
                var xp = Calculate.ReturnXP(msg);
                var credit = Calculate.ReturnCredit();
                var levelupReq = Calculate.CalculateNextLevel(1);

                if (xp >= levelupReq)
                {

                }
                else
                {

                }
            });
        }

    }
}
