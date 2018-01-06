using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.Automate.Cleanup
{
    /*
    public class CleanUp : IJob
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;
        public CleanUp(DiscordSocketClient discord, IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            MessageCleanUp();
            return Task.CompletedTask;
        }

        private Task MessageCleanUp()
        {
            var _ = Task.Run(async () =>
            {
                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                var ch = guild.TextChannels.First(x => x.Id == 339380254097539072);
                var msgs = ch.CachedMessages;
                await Task.WhenAll(Task.Delay(1000), ch.DeleteMessagesAsync(msgs)).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }
    }
    */
}