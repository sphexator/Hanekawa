using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.AnimeSimulCast.Events;
using Hanekawa.AnimeSimulCast.Extensions;

namespace Hanekawa.AnimeSimulCast
{
    public class AnimeSimulCastClient
    {
        private SyndicationItem LastItem;

        public AnimeSimulCastClient()
        {
            Initialize();
        }

        public event AsyncEvent<AnimeData> AnimeAired;

        public Task StartAsync()
        {
            _ = MainAsync(new CancellationToken());
            return Task.CompletedTask;
        }

        private void Initialize()
        {
            var reader = XmlReader.Create(Constants.RssFeed);
            var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
            LastItem = feed;
        }

        private async Task MainAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var feed = SyndicationFeed.Load(XmlReader.Create(Constants.RssFeed)).Items.FirstOrDefault();

                    if (feed != LastItem)
                    {
                        UpdatePoll(feed);
                        _ = AnimeAired(ToReturnType(feed));
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                }
            }
        }

        private static AnimeData ToReturnType(SyndicationItem collection)
        {
            var data = new AnimeData
            {
                Title = collection.Title.Text.Filter(),
                Time = collection.PublishDate,
                Episode = collection.Title.Text.GetEpisode(),
                Season = collection.Title.Text.GetSeason(),
                Url = collection.Links.FirstOrDefault().Uri.AbsoluteUri
            };
            return data;
        }

        private void UpdatePoll(SyndicationItem item) => LastItem = item;
    }
}