using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Modules.Administration.Services;
using Quartz;

namespace Jibril.Services.Automate.Ban
{
    public class BanScheduler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public BanScheduler(DiscordSocketClient discord, IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;
        }

        /*
        public Task Execute(IJobExecutionContext context)
        {
            UnbanUser();
            return Task.CompletedTask;
        }

        private Task UnbanUser()
        {
            var _ = Task.Run( async() =>
            {
                var users = AdminDb.GetBannedUsers();
                if (users == null) return;
                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                foreach (var user in users)
                {
                    await guild.RemoveBanAsync(user);
                    await Task.Delay(2500);
                }
            });
            return Task.CompletedTask;
        }
        */
    }
}