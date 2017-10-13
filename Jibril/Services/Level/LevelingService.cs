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
            _discord.UserVoiceStateUpdated += VoiceExp;
        }

        public Task GiveExp(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                var user = msg.Author as SocketGuildUser;
                var guild = user.Guild.Id;

                var CheckUser = DatabaseService.CheckUser(user);
                if (CheckUser == null) DatabaseService.EnterUser(user);

                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var exp = Calculate.ReturnXP(msg);
                var credit = Calculate.ReturnCredit();
                var levelupReq = Calculate.CalculateNextLevel(1);
                var cooldownCheck = Cooldown.ExperienceCooldown(userData.Cooldown);
                if (cooldownCheck == true && user.IsBot != true)
                {
                    LevelDatabase.ChangeCooldown(user);
                    LevelDatabase.AddExperience(user, exp, credit);
                    if ((userData.Xp + exp) >= levelupReq)
                    {
                        var remainingExp = userData.Xp - exp;
                        LevelDatabase.Levelup(user, remainingExp);
                        await LevelRoles.AssignNewRole(user, userData.Level);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public Task VoiceExp(SocketUser usr, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(() =>
            {
                var gusr = usr as IGuildUser;
                var oldVc = oldState.VoiceChannel;
                var newVc = newState.VoiceChannel;
                try
                {
                    var CheckUser = DatabaseService.CheckUser(gusr);
                    if (CheckUser == null) DatabaseService.EnterUser(gusr);
                    if (newVc != null && oldVc == null)
                    {
                        LevelDatabase.StartVoiceCounter(gusr);
                    }
                    if (oldVc != null && newVc == null)
                    {
                        var userInfo = DatabaseService.UserData(gusr).FirstOrDefault();
                        Calculate.VECC(gusr, userInfo.Voice_timer);
                    }
                }
                catch
                {

                }
            });
            return Task.CompletedTask;
        }

    }
}
