using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Hanekawa.Bot
{
    public class Hanekawa : BackgroundService
    {
        private readonly DiscordBot _client;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _provider;

        public Hanekawa(DiscordBot client, IServiceProvider provider, IConfiguration config)
        {
            _client = client;
            _provider = provider;
            _config = config;
        }

        private void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var serviceList = assembly.GetTypes()
                    .Where(x => x.GetInterfaces().Contains(typeof(IRequired))
                                && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
                for (var i = 0; i < serviceList.Count; i++) _provider.GetRequiredService(serviceList[i]);

                // _provider.GetRequiredService<CommandHandlingService>().InitializeAsync();
                var scheduler = _provider.GetRequiredService<IScheduler>();
                QuartzExtension.StartCronJob<WarnService>(scheduler, "0 0 13 1/1 * ? *");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Initialize();
            await _client.AddExtensionAsync(new InteractivityExtension());
            await _client.RunAsync(stoppingToken);
            await Task.Delay(-1, stoppingToken);
        }
    }
}