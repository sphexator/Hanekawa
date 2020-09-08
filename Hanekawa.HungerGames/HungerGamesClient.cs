using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hanekawa.HungerGames.Entities;
using Hanekawa.HungerGames.Entities.Internal;
using Hanekawa.HungerGames.Entities.Items;
using Hanekawa.HungerGames.Entities.Result;
using Hanekawa.HungerGames.Entities.User;
using Hanekawa.HungerGames.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace HungerGame
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
            services.AddSingleton(_config.HttpClient);
            services.AddSingleton(_config.Random);

            var assembly = Assembly.GetAssembly(typeof(HungerGamesClient));
            var requiredServices = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IRequired))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            foreach (var x in requiredServices) services.AddSingleton(x);
            return services.BuildServiceProvider();
        }
    }
}