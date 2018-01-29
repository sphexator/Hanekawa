using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.Interactivity
{
    public class I_am_infamous : IJob
    {
        private readonly DiscordSocketClient _client;
        private List<CooldownUser> _users = new List<CooldownUser>(); 
        public I_am_infamous(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += MessageCounter;
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
                var guild = _client.Guilds.FirstOrDefault(x => x.Id == 200265036596379648);
                var role = guild?.Roles.FirstOrDefault(x => x.Name == "Wolfy");
                var oldMvps = role?.Members;
                var ma = DatabaseService.GetActiveUsers();
                var newMvps = new List<SocketGuildUser>();
                foreach (var x in ma)
                {
                    var user = guild?.GetUser(x);
                    newMvps.Add(user);
                }

                await Demote(oldMvps, guild, role);
                await Promote(newMvps, guild, role);
            });
            return Task.CompletedTask;
        }

        private async Task Demote(IEnumerable<SocketGuildUser> mvps, SocketGuild guild, SocketRole role)
        {
            foreach (var x in mvps) await x.RemoveRoleAsync(role);
        }

        private async Task Promote(IEnumerable<SocketGuildUser> mvps, SocketGuild guild, SocketRole role)
        {
            foreach (var x in mvps) await x.AddRoleAsync(role);
        }

        private Task MessageCounter(SocketMessage msg)
        {
            var _ = Task.Run(() =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                var cd = CheckCooldownAsync(msg.Author as SocketGuildUser);
                if (cd == false) return;
                Console.WriteLine($"{DateTime.UtcNow.Second} - Didn't return");
                DatabaseService.AddMessageCounter(msg.Author);
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldownAsync(SocketGuildUser usr)
        {
            var tempUser = _users.FirstOrDefault(x => x.User == usr);
            if (tempUser != null)// check to see if you have handled a request in the past from this user.
            {
                if ((DateTime.Now - tempUser.LastRequest).TotalSeconds >= 60) // checks if more than 30 seconds have passed between the last requests send by the user
                {
                    _users.Find(x => x.User == usr).LastRequest = DateTime.Now; // update their last request time to now.
                    return true;
                }

                return false;
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