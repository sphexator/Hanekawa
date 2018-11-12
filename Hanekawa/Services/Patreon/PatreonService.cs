using Discord.WebSocket;
using Hanekawa.Addons.Patreon;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Core.Extensions;

namespace Hanekawa.Services.Patreon
{
    public class PatreonService : IJob
    {
        private readonly PatreonClient _patreonClient;
        private readonly DiscordSocketClient _client;

        public PatreonService(DiscordSocketClient client, PatreonClient patreonClient)
        {
            _client = client;
            _patreonClient = patreonClient;
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public async Task Execute()
        {
            await PatreonRewardsAsync();
        }

        private async Task PatreonRewardsAsync()
        {
            var totalPledges = await _patreonClient.GetPledges();
            var pledges = totalPledges.Where(x =>
                x.Pledges.DeclinedSince == null || ((x.Pledges.DeclinedSince.Value.Month == DateTime.UtcNow.Month &&
                                                     x.Pledges.DeclinedSince.Value.Year == DateTime.UtcNow.Year) &&
                                                    (x.Pledges.CreatedAt.Month < DateTime.UtcNow.Month &&
                                                     x.Pledges.CreatedAt.Year <= DateTime.UtcNow.Year)));
            pledges.ForEach(x => Console.WriteLine($"{x.Users.DiscordId} - {x.Pledges.AmountCents}"));
        }
    }
}
