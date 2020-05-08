using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hanekawa.Bot
{
    public class Hanekawa : BackgroundService
    {
        private readonly DiscordBot _client;

        public Hanekawa(DiscordBot client) => _client = client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var assembly = Assembly.GetEntryAssembly();
            _client.AddModules(assembly);
            await _client.AddExtensionAsync(new InteractivityExtension());
            await _client.RunAsync(stoppingToken);
            await Task.Delay(-1, stoppingToken);
        }
    }
}