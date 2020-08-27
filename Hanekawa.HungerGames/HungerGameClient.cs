using System;
using Hanekawa.HungerGames.Entity.Event;
using Hanekawa.HungerGames.Generator;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.HungerGames
{
    public class HungerGameClient
    {
        private HungerGameConfig _config;
        private IServiceProvider _provider;
        public HungerGameClient(HungerGameConfig config) => _config = config;

        public void StartGame(){}

        public void NextRound()
        {
            // TODO: Manage a way to eat
        }

        public void Initialize() =>
            _provider = new ServiceCollection()
                .AddSingleton(_config)
                .AddScoped<Chance>()
                .AddScoped<Attack>()
                .AddScoped<Die>()
                .AddScoped<Hack>()
                .AddScoped<Idle>()
                .AddScoped<Loot>()
                .AddScoped<Sleep>()
                .AddScoped<EventHandler>()
                .AddScoped<Random>()
                .BuildServiceProvider();
    }
}