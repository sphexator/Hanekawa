using Jibril.Services.Level.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using Discord;
using Discord.Commands;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        public DiscordSocketClient _client { get; }
        private IServiceProvider _provider;

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _client = discord;
            _provider = provider;

            _client.MessageReceived += GiveExp;
            _client.UserVoiceStateUpdated += VoiceExp;
        }

        private Task GiveExp(SocketMessage msg)
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
                DateTime cooldown = Convert.ToDateTime(userData.Cooldown);
                var levelupReq = Calculate.CalculateNextLevel(userData.Level);
                var cooldownCheck = Cooldown.ExperienceCooldown(cooldown);
                if (cooldownCheck == true && user.IsBot != true)
                {
                    LevelDatabase.ChangeCooldown(user);
                    LevelDatabase.AddExperience(user, exp, credit);
                    Console.WriteLine($"{DateTime.Now.Hour}:{DateTime.Now.Minute} | LEVEL SERVICE   |   {msg.Author.Username} Recieved {exp} exp");
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

        private Task VoiceExp(SocketUser usr, SocketVoiceState oldState, SocketVoiceState newState)
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
                        DateTime cooldown = Convert.ToDateTime(userInfo.Voice_timer);
                        Calculate.VECC(gusr, cooldown);
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
