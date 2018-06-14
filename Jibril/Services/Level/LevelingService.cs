using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Level.Services;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private readonly IServiceProvider _provider;
        private readonly List<CooldownUser> _users = new List<CooldownUser>();

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
                using (var db = new hanekawaContext())
                {
                    var userdata = await db.Exp.FindAsync(usr.Id.ToString());
                    if (userdata == null) return Task.CompletedTask;
                    if (userdata.Level <= 2) return Task.CompletedTask;
                    await LevelRoles.AssignRoles(userdata, usr);
                }

                return null;
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

                var cd = CheckCooldown(user);
                if (cd == false) return;
                using (var db = new hanekawaContext())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    var exp = Calculate.MessageExperience(msg);
                    var credit = Calculate.MessageCredit();
                    var lvlupReq = Calculate.CalculateNextLevel(userdata.Level);

                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} | LEVEL SERVICE | Awarded {exp} exp to {msg.Author.Username}");
                    if (userdata.Xp + exp >= lvlupReq)
                    {
                        userdata.Xp = userdata.Xp + exp - lvlupReq;
                        userdata.Tokens = userdata.Tokens + credit;
                        userdata.TotalXp = userdata.TotalXp + exp;
                        userdata.Level = userdata.Level + 1;
                        await db.SaveChangesAsync();

                        await LevelRoles.AssignNewRole(user, userdata.Level);
                    }
                    else
                    {
                        userdata.Xp = userdata.Xp + exp;
                        userdata.Tokens = userdata.Tokens + credit;
                        userdata.TotalXp = userdata.TotalXp + exp;
                        await db.SaveChangesAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static Task VoiceExp(SocketUser usr, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                var user = usr as IGuildUser;
                var oldVc = oldState.VoiceChannel;
                var newVc = newState.VoiceChannel;
                try
                {
                    using (var db = new hanekawaContext())
                    {
                        var userdata = await db.GetOrCreateUserData(user);
                        if (newVc != null && oldVc == null)
                        {
                            userdata.VoiceTimer = DateTime.UtcNow;
                            await db.SaveChangesAsync();
                            return;
                        }
                        if (oldVc == null || newVc != null) return;
                        if (userdata.VoiceTimer != null)
                        {
                            var xp = Calculate.CalculateVoiceExperience(userdata.VoiceTimer.Value) * 1;
                            if (xp < 0) return;
                            var credit = Calculate.CalculateVoiceCredit(userdata.VoiceTimer.Value);
                            var lvlupReq = Calculate.CalculateNextLevel(userdata.Level);

                            if (userdata.Xp + xp >= lvlupReq)
                            {
                                userdata.Xp = userdata.Xp + xp - lvlupReq;
                                userdata.Tokens = userdata.Tokens + credit;
                                userdata.TotalXp = userdata.TotalXp + xp;
                                userdata.Level = userdata.Level + 1;
                                await db.SaveChangesAsync();

                                await LevelRoles.AssignNewRole(user, userdata.Level);
                            }
                            else
                            {
                                userdata.Xp = userdata.Xp + xp;
                                userdata.Tokens = userdata.Tokens + credit;
                                userdata.TotalXp = userdata.TotalXp + xp;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch{/*Ignore */}
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