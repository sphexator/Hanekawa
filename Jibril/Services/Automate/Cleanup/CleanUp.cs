using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.Automate.Cleanup
{
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
                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208).GetTextChannel(339380254097539072);
                var msgs = guild.CachedMessages.Where(m => m.Author.Id != 111123736660324352).Take(50).ToArray();

                if (msgs.Length == 0) return;

                var bulkDeletable = new List<IMessage>();
                foreach (var x in msgs)
                    bulkDeletable.Add(x);

                var channel = guild;
                await Task.WhenAll(Task.Delay(1000), channel.DeleteMessagesAsync(bulkDeletable)).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }
    }
}