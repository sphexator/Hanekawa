using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Parsers;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.Hosting;

namespace Hanekawa.Bot
{
    public class Hanekawa : BackgroundService
    {
        private readonly DiscordBot _client;
        private bool _setup = false;

        public Hanekawa(DiscordBot client) => _client = client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.AddExtensionAsync(new InteractivityExtension());
            await _client.RunAsync(stoppingToken);
            if(!_setup) Setup();
            await Task.Delay(-1, stoppingToken);
        }

        private void Setup()
        {
            var assembly = Assembly.GetEntryAssembly();
            _client.RemoveTypeParser(_client.GetTypeParser<CachedRoleTypeParser>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedMemberTypeParser>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedUserTypeParser>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedGuildChannelTypeParser<CachedGuildChannel>>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedGuildChannelTypeParser<CachedTextChannel>>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedGuildChannelTypeParser<CachedVoiceChannel>>());
            _client.RemoveTypeParser(_client.GetTypeParser<CachedGuildChannelTypeParser<CachedCategoryChannel>>());

            _client.AddTypeParser(new CachedRoleTypeParser(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedMemberTypeParser(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedUserTypeParser(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedGuildChannelTypeParser<CachedGuildChannel>(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedGuildChannelTypeParser<CachedTextChannel>(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedGuildChannelTypeParser<CachedVoiceChannel>(StringComparison.OrdinalIgnoreCase));
            _client.AddTypeParser(new CachedGuildChannelTypeParser<CachedCategoryChannel>(StringComparison.OrdinalIgnoreCase));

            _client.AddModules(assembly);
            _setup = true;
        }
    }
}