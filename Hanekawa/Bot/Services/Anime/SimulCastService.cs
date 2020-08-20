using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Anime
{
    public class SimulCastService : BackgroundService, INService, IRequired
    {
        private readonly Hanekawa _client;
        private readonly InternalLogService _log;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        private const string RssFeed = "https://www.crunchyroll.com/rss/anime?lang=enGB";
        private string _lastItem;

        public SimulCastService(Hanekawa client, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _log = log;
            _provider = provider;
            _colourService = colourService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var reader = XmlReader.Create(RssFeed))
                {
                    var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                    if (feed != null)
                    {
                        _lastItem = feed.Id;
                    }
                }
            }
            catch
            {
                // ignored
            }

            while (stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var reader = XmlReader.Create(RssFeed);
                    var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                    feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                    if (feed == null) return;
                    if (feed.Id == _lastItem) return;
                    feed.Id = _lastItem;
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var premiumList = await db.GuildConfigs.Where(x => x.Premium).ToListAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
                    for (var i = 0; i < premiumList.Count; i++)
                    {
                        var x = premiumList[i];
                        try
                        {
                            var data = new AnimeData
                            {
                                Title = feed.Title.Text,
                                Time = feed.PublishDate
                            };
                            var url = feed.Links.FirstOrDefault();
                            if (url != null) data.Url = url.Uri.AbsoluteUri;
                            await PostAsync(x, data);
                        }
                        catch (Exception e)
                        {
                            _log.LogAction(LogLevel.Error, e, $"(Anime Cast Service) Unable to post to {x.GuildId}");
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e, "(Anime Cast Service) Error reading feed");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task PostAsync(GuildConfig cfg, AnimeData data)
        {
            try
            {
                if (!cfg.AnimeAirChannel.HasValue) return;
                var guild = _client.GetGuild(cfg.GuildId);
                if (guild == null) return;
                _log.LogAction(LogLevel.Information, $"Posting anime event to {guild.Name}");
                var channel = guild.GetTextChannel(cfg.AnimeAirChannel.Value);
                if (channel == null)
                {
                    cfg.AnimeAirChannel = null;
                    return;
                }
                await channel.ReplyAsync(BuildEmbed(data, cfg.GuildId));
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

    public class AnimeData
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}