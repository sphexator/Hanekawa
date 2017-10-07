using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Services.Level.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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
            await Task.Run(async () =>
            {
                var user = msg.Author as SocketGuildUser;
                var guild = user.Guild.Id;

                var CheckUser = DatabaseService.CheckUser(user);
                if (CheckUser == null)
                {
                    DatabaseService.EnterUser(user);
                }

                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var exp = Calculate.ReturnXP(msg);
                var credit = Calculate.ReturnCredit();
                var levelupReq = Calculate.CalculateNextLevel(1);
                var cooldownCheck = Cooldown.ExperienceCooldown(userData.Cooldown);
                if (cooldownCheck == true && user.IsBot != true)
                {
                    DbService.AddExperience(user, exp, credit);
                    if ((userData.Xp + exp) >= levelupReq)
                    {
                        var remainingExp = userData.Xp - exp;
                        DbService.Levelup(user, remainingExp);
                        await LevelRoles.AssignNewRole(user, userData.Level);
                    }
                }
            });
        }

    }
}
