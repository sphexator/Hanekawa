using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.AnimeSimulCast;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Anime
{
    public class SimulCastService : INService, IRequired
    {
        private readonly AnimeSimulCastClient _anime;
        private readonly DiscordClient _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        public SimulCastService(AnimeSimulCastClient anime, DiscordClient client, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _anime = anime;
            _client = client;
            _log = log;
            _provider = provider;
            _colourService = colourService;

            _anime.AnimeAired += AnimeAired;
            _client.Ready += SetupSimulCast;
        }

        private Task SetupSimulCast(ReadyEventArgs e)
        {
            _ = _anime.Start();
            return Task.CompletedTask;
        }

        private Task AnimeAired(AnimeData data)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var premiumList = await db.GuildConfigs.Where(x => x.Premium).ToListAsync().ConfigureAwait(false);
                    foreach (var x in premiumList)
                    {
                        await PostAsync(x, data).ConfigureAwait(false);
                        await Task.Delay(5000).ConfigureAwait(false);
                    }
                    _log.LogAction(LogLevel.Information, $"(Anime Simulcast) Announced {data.Title} in {premiumList.Count} guilds");
                }
            });
            return Task.CompletedTask;
        }

        private async Task PostAsync(GuildConfig cfg, AnimeData data)
        {
            try
            {
                if (!cfg.AnimeAirChannel.HasValue) return;
                var guild = _client.GetGuild(cfg.GuildId);
                if (guild == null) return;
                _log.LogAction(LogLevel.Information, $"Posting anime event to {guild.Name}");
                await guild.GetTextChannel(cfg.AnimeAirChannel.Value)
                    .ReplyAsync(BuildEmbed(data, cfg.GuildId));
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Anime Simulcast) Error for {cfg.GuildId} - {e.Message}");
            }
        }

        private LocalEmbedBuilder BuildEmbed(AnimeData data, ulong guild)
        {
            var embed = new LocalEmbedBuilder()
                .Create(null, _colourService.Get(guild))
                .WithAuthor(new LocalEmbedAuthorBuilder {Name = "New Episode Available!"})
                .WithTitle($"{data.Title}")
                .WithUrl(data.Url)
                .WithTimestamp(data.Time);
            return embed;
        }
    }
}