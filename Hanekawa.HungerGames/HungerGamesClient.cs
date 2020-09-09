using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.HungerGames.Entities;
using Hanekawa.HungerGames.Entities.Internal.Events;
using Hanekawa.HungerGames.Entities.Items;
using Hanekawa.HungerGames.Entities.Result;
using Hanekawa.HungerGames.Entities.User;
using Hanekawa.HungerGames.Generator;
using Hanekawa.HungerGames.Handler;
using Hanekawa.HungerGames.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.HungerGames
{
    public class HungerGamesClient
    {
        private readonly HungerGameConfig _config;
        private readonly IServiceProvider _service;

        public HungerGamesClient()
        {
            _config = new HungerGameConfig();
            _service = ConfigureServices();
        }

        public HungerGamesClient(HungerGameConfig cfg)
        {
            _config = cfg;
            _service = ConfigureServices();
        }

        public async Task<IEnumerable<HgResult>> PlayCustomAsync(List<HungerGameProfile> profiles, ItemDrop itemDrops) =>
            await _service.GetRequiredService<GameHandler>().CustomRoundAsync(profiles, itemDrops);

        public async Task<HgOverallResult> PlayDefaultAsync(List<HungerGameProfile> profiles, ItemDrop itemDrops) =>
            await _service.GetRequiredService<GameHandler>().DefaultRoundAsync(profiles, itemDrops);

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new HttpClient());
            services.AddSingleton(new Random());
            // Generators 
            services.AddTransient<ChanceGenerator>();
            services.AddTransient<ImageGenerator>();
            // Handlers
            services.AddTransient<Hanekawa.HungerGames.Handler.EventHandler>();
            services.AddTransient<GameHandler>();
            // Util
            services.AddTransient<DamageOutput>();
            services.AddTransient<Image>();
            // Events
            services.AddTransient<Attack>();
            services.AddTransient<Consume>();
            services.AddTransient<Die>();
            services.AddTransient<Hack>();
            services.AddTransient<Idle>();
            services.AddTransient<Loot>();
            services.AddTransient<Meet>();
            services.AddTransient<Sleep>();
            return services.BuildServiceProvider();
        }
    }
}