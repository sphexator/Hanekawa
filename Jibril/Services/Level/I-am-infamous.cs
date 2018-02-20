using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.Level
{
    public class I_am_infamous : IJob
    {
        private readonly DiscordSocketClient _client;
        private List<CooldownUser> _users = new List<CooldownUser>();
        private List<ulong> _channels = new List<ulong>();
        public I_am_infamous(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += MessageCounter;

            _channels.Add(339371997802790913); //General
            _channels.Add(351861569530888202); //Tea-room
            _channels.Add(341904875363500032); //Gaming
            _channels.Add(353306001858101248); //Anime
            _channels.Add(382920381985456129); //tech
            _channels.Add(353306043373322252); //music

            _channels.Add(404633037884620802); //Test channel
        }

        public Task Execute(IJobExecutionContext context)
        {
            NewMvpUsers();
            return Task.CompletedTask;
        }

        private Task NewMvpUsers()
        {
            var _ = Task.Run(async () =>
            {
                var guild = _client.Guilds.FirstOrDefault(x => x.Id == 339370914724446208);
                var role = guild?.Roles.FirstOrDefault(x => x.Name == "Kai Ni");
                var oldMvps = role?.Members;
                var ma = DatabaseService.GetActiveUsers();
                var newMvps = new List<SocketGuildUser>();
                foreach (var x in ma)
                {
                    var user = guild?.GetUser(x);
                    newMvps.Add(user);
                }
                await Demote(oldMvps, role);
                await Promote(newMvps, role);
                DatabaseService.ResetMessageCounter();
            });
            return Task.CompletedTask;
        }

        private static async Task Demote(IEnumerable<SocketGuildUser> mvps, IRole role)
        {
            foreach (var x in mvps)
            {
                try
                {
                    await x.RemoveRoleAsync(role);
                    await Task.Delay(1000);
                }
                catch
                {
                    //Ignore
                }
            }
        }

        private static async Task Promote(IEnumerable<SocketGuildUser> mvps, IRole role)
        {
            foreach (var x in mvps)
            {
                try
                {
                    await x.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
                catch
                {
                    //Ignore
                }
            }
        }

        private Task MessageCounter(SocketMessage msg)
        {
            var _ = Task.Run(() =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!_channels.Contains(msg.Channel.Id)) return;
                var cd = CheckCooldownAsync(msg.Author as SocketGuildUser);
                if (cd == false) return;
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | MVP SERVICE | +1 {msg.Author.Username}");
                DatabaseService.AddMessageCounter(msg.Author);
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldownAsync(SocketGuildUser usr)
        {
            var tempUser = _users.FirstOrDefault(x => x.User == usr);
            if (tempUser != null)// check to see if you have handled a request in the past from this user.
            {
                if (!((DateTime.Now - tempUser.LastRequest).TotalSeconds >= 60)) return false;
                _users.Find(x => x.User == usr).LastRequest = DateTime.Now; // update their last request time to now.
                return true;

            }

            var newUser = new CooldownUser
            {
                User = usr,
                LastRequest = DateTime.Now
            };
            _users.Add(newUser);
            return true;
        }
    }

    public class CooldownUser
    {
        public SocketGuildUser User { get; set; }
        public DateTime LastRequest { get; set; }
    }
}