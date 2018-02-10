﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.Level.Lists;
using Jibril.Services.Level.Services;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private IServiceProvider _provider;

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord)
        {
            var client = discord;
            _provider = provider;

            client.MessageReceived += GiveExp;
            client.UserVoiceStateUpdated += VoiceExp;
            client.UserJoined += GiveRolesBack;
        }

        private Task GiveRolesBack(SocketGuildUser usr)
        {
            var _ = Task.Run(async () =>
            {
                var userdata = DatabaseService.UserData(usr).FirstOrDefault();
                if (userdata == null) return;
                if (userdata.Level <= 2) return;

            });
            return Task.CompletedTask;
        }

        private Task GiveExp(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                var user = msg.Author as SocketGuildUser;
                var guild = user.Guild.Id;

                var CheckUser = DatabaseService.CheckUser(user).FirstOrDefault();
                if (CheckUser == null) DatabaseService.EnterUser(user);

                var userData = DatabaseService.UserData(user).FirstOrDefault();
                var exp = Calculate.ReturnXP(msg);
                var credit = Calculate.ReturnCredit();
                var cooldown = Convert.ToDateTime(userData.Cooldown);
                var levelupReq = Calculate.CalculateNextLevel(userData.Level);
                var cooldownCheck = Cooldown.ExperienceCooldown(cooldown);
                if (cooldownCheck && user.IsBot != true)
                {
                    LevelDatabase.ChangeCooldown(user);
                    LevelDatabase.AddExperience(user, exp, credit);
                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} | LEVEL SERVICE | Awarded {exp} exp to {msg.Author.Username}");
                    if (userData.Xp + exp >= levelupReq)
                    {
                        var remainingExp = userData.Xp + exp - userData.Xp;
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
                    var CheckUser = DatabaseService.CheckUser(gusr).FirstOrDefault();
                    if (CheckUser == null) DatabaseService.EnterUser(gusr);
                    if (newVc != null && oldVc == null)
                        LevelDatabase.StartVoiceCounter(gusr);
                    if (oldVc != null && newVc == null)
                    {
                        var userInfo = DatabaseService.UserData(gusr).FirstOrDefault();
                        var cooldown = Convert.ToDateTime(userInfo.Voice_timer);
                        Calculate.VECC(gusr, cooldown);
                    }
                }
                catch
                {
                    //Ignore
                }
            });
            return Task.CompletedTask;
        }
    }
}