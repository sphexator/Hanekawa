using System;
using System.Threading.Tasks;
using Hanekawa.Shared.Interfaces;
using Quartz;

namespace Hanekawa.Bot.Services.HungerGames
{
    public class HungerGameService : INService, IRequired, IJob
    {
        private readonly Hanekawa _client;
        private readonly IServiceProvider _provider;

        public HungerGameService(Hanekawa client, IServiceProvider provider)
        {
            _client = client;
            _provider = provider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = Task.Run(async () =>
            {

            });
            return Task.CompletedTask;
        }

        private async Task StartSignUpAsync(){}
        private async Task StartGameAsync(){}
        private async Task NextRoundAsync(){}
    }
}