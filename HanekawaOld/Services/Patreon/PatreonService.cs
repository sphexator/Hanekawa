using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Patreon;
using Hanekawa.Entities.Interfaces;
using Quartz;

namespace Hanekawa.Services.Patreon
{
    public class PatreonService : IJob, IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly PatreonClient _patreonClient;

        public PatreonService(DiscordSocketClient client, PatreonClient patreonClient)
        {
            _client = client;
            _patreonClient = patreonClient;
            Console.WriteLine("Patreon service loaded");
        }

        public Task Execute(IJobExecutionContext context) => throw new NotImplementedException();

        public async Task Execute()
        {
            await PatreonRewardsAsync();
        }

        private async Task PatreonRewardsAsync()
        {
            var totalPledges = await _patreonClient.GetPledges();
            var pledges = totalPledges.Where(x =>
                x.Pledges.DeclinedSince == null || x.Pledges.DeclinedSince.Value.Month == DateTime.UtcNow.Month &&
                x.Pledges.DeclinedSince.Value.Year == DateTime.UtcNow.Year &&
                x.Pledges.CreatedAt.Month < DateTime.UtcNow.Month && x.Pledges.CreatedAt.Year <= DateTime.UtcNow.Year);
            foreach (var x in pledges) Console.WriteLine($"{x.Users.DiscordId} - {x.Pledges.AmountCents}");
        }
    }
}