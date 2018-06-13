using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.Level.Services;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private readonly IServiceProvider _provider;
        private List<CooldownUser> _users = new List<CooldownUser>();

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord)
        {
            var client = discord;
            _provider = provider;

            client.MessageReceived += GiveExp;
            client.UserVoiceStateUpdated += VoiceExp;
            client.UserJoined += GiveRolesBack;
        }

        private static Task GiveRolesBack(SocketGuildUser usr)
        {
            var _ = Task.Run(async () =>
            {
                var userdata = DatabaseService.UserData(usr).FirstOrDefault();
                if (userdata == null) return;
                if (userdata.Level <= 2) return;
                await LevelRoles.AssignRoles(userdata, usr);
            });
            return Task.CompletedTask;
        }

        private Task GiveExp(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                var user = msg.Author as SocketGuildUser;
                if (user.IsBot) return;
                if ((msg.Channel as ITextChannel)?.CategoryId == 441660828379381770) return;
                var checkUser = DatabaseService.CheckUser(user).FirstOrDefault();
                if (checkUser == null) DatabaseService.EnterUser(user);

                var cd = CheckCooldown(user);
                if (cd == false) return;
                var userdata = DatabaseService.UserData(user).FirstOrDefault();
                if(userdata.FirstMsg.ToString("yyyy-MM-dd HH:mm") == "0001-01-01 00:00") DatabaseService.AddFirstMessage(user);
                var exp = Calculate.ReturnXP(msg);
                var credit = Calculate.ReturnCredit();
                var lvlupReq = Calculate.CalculateNextLevel(userdata.Level);

                LevelDatabase.AddExperience(user, exp, credit);
                Console.WriteLine(
                    $"{DateTime.Now.ToLongTimeString()} | LEVEL SERVICE | Awarded {exp} exp to {msg.Author.Username}");
                if (userdata.Xp + exp >= lvlupReq)
                {
                    var remainingExp = userdata.Xp + exp - lvlupReq;
                    LevelDatabase.Levelup(user, remainingExp);
                    await LevelRoles.AssignNewRole(user, userdata.Level);
                }
            });
            return Task.CompletedTask;
        }

        private static Task VoiceExp(SocketUser usr, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(() =>
            {
                var gusr = usr as IGuildUser;
                var oldVc = oldState.VoiceChannel;
                var newVc = newState.VoiceChannel;
                try
                {
                    var checkUser = DatabaseService.CheckUser(gusr).FirstOrDefault();
                    if (checkUser == null) DatabaseService.EnterUser(gusr);
                    if (newVc != null && oldVc == null)
                        LevelDatabase.StartVoiceCounter(gusr);
                    if (oldVc == null || newVc != null) return;
                    var userInfo = DatabaseService.UserData(gusr).FirstOrDefault();
                    var cooldown = Convert.ToDateTime(userInfo.Voice_timer);
                    Calculate.VECC(gusr, cooldown);
                }
                catch
                {
                    //Ignore
                }
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldown(SocketGuildUser usr)
        {
            var tempUser = _users.FirstOrDefault(x => x.User == usr);
            if (tempUser != null)// check to see if you have handled a request in the past from this user.
            {
                if (!((DateTime.Now - tempUser.LastRequest).TotalSeconds >= 60)) return false;
                _users.Find(x => x.User == usr).LastRequest = DateTime.Now; // update their last request time to now.
                return true;
            }

            var newUser = new CooldownUser()
            {
                User = usr,
                LastRequest = DateTime.Now
            };
            _users.Add(newUser);
            return true;
        }
    }
}